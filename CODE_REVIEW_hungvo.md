# Code Review — TodoMVC (Angular 20 + ASP.NET Core 10 + MongoDB + Azure Service Bus)

> **Phạm vi**: review toàn bộ codebase tại `ng-hungho` (commit `e7017df`, nhánh `main`).
> **Lưu ý**: theo yêu cầu, tài liệu này **chỉ liệt kê và phân tích lỗi/vấn đề**, **không kèm hướng dẫn hay prompt sửa lỗi**.

---

## 1. Tổng quan hệ thống

| Thành phần | Công nghệ | Vị trí | Cổng |
|---|---|---|---|
| `Todo.Api` | ASP.NET Core 10, Minimal API + Carter, MediatR (CQRS), FluentValidation, MongoDB.Entities, Azure Service Bus | [src/Todo.Api/](src/Todo.Api/) | 5200 |
| `Todo.Bff` | ASP.NET Core 10, Carter, typed `HttpClient` proxy | [src/Todo.Bff/](src/Todo.Bff/) | 5100 |
| `todo-app` | Angular 20 (standalone + SSR), NgRx ComponentStore, RxJS, todomvc-app-css | [src/todo-app/](src/todo-app/) | 4200 |
| Hạ tầng | MongoDB 7 (Docker), Azure Service Bus Emulator | [docker-compose.yaml](docker-compose.yaml) | 27017 |

**Luồng dữ liệu**: Angular → BFF (`/bff/*`) → API (`/api/*`) → MongoDB. Reminder chạy qua ASB Scheduled Message → `ReminderProcessor` → tạo `Reminder` → đẩy SSE về BFF → relay xuống Angular (`EventSource`).

**Kiến trúc code**: Vertical Slice + REPR pattern (Request-Endpoint-Response) khá nhất quán ở cả API và BFF — mỗi feature một thư mục gồm Endpoint / Command hoặc Query / Handler / Validator / Response.

### Cách kiểm chứng khi review

- `dotnet build Todo/Todo.slnx` → **Build succeeded, 0 Errors, 18 Warnings** (chi tiết ở Phụ lục A).
- Frontend **chưa build/test được**: `node_modules` chưa được cài trong workspace, nên các phát hiện phía Angular là kết quả đọc code tĩnh, chưa qua compiler/runtime.

---

## 2. Bảng tóm tắt

| # | Mức độ | Vấn đề | Vị trí |
|---|---|---|---|
| C1 | 🔴 Nghiêm trọng | `ReminderScanner` không được đăng ký → reminder đã Snooze không bao giờ quay lại Pending | [Program.cs:33](src/Todo.Api/Program.cs#L33) |
| C2 | 🔴 Nghiêm trọng | Đổi `DueAt` khi Update không dọn `Reminder` cũ → reminder mới không bao giờ được tạo | [UpdateTodoHandler.cs:37](src/Todo.Api/Features/Todos/Update/UpdateTodoHandler.cs#L37), [ReminderProcessor.cs:54](src/Todo.Api/Features/Reminders/ReminderProcessor.cs#L54) |
| C3 | 🔴 Nghiêm trọng | Không thể sửa tiêu đề todo đã quá hạn (validator chặn `DueAt` quá khứ) | [UpdateTodoCommandValidator.cs:15](src/Todo.Api/Features/Todos/Update/UpdateTodoCommandValidator.cs#L15), [todo-item.ts:51](src/todo-app/src/features/todos/todo-item/todo-item.ts#L51) |
| C4 | 🔴 Nghiêm trọng | Xử lý múi giờ không nhất quán (`datetime-local` không timezone vs `DateTime.UtcNow`) | FE→API toàn tuyến |
| C5 | 🔴 Nghiêm trọng | `/api/stats/overview` ném `NotImplementedException` → HTTP 500 | [StatsOverviewHandler.cs:24](src/Todo.Api/Features/Statistics/StatsOverviewHandler.cs#L24) |
| H1 | 🟠 Cao | `DeleteCompleted` không dismiss Reminder, không hủy lịch ASB → chuông hiện reminder mồ côi | [DeleteCompletedTodosHandler.cs:14](src/Todo.Api/Features/Todos/DeleteCompleted/DeleteCompletedTodosHandler.cs#L14) |
| H2 | 🟠 Cao | `DeleteTodo` không hủy Scheduled Message trên ASB → rác trong queue | [DeleteTodoHandler.cs:24](src/Todo.Api/Features/Todos/Delete/DeleteTodoHandler.cs#L24) |
| H3 | 🟠 Cao | Bóc lỗi validation ở FE sai cấu trúc → user thấy `undefined` khi Snooze lỗi | [reminder.store.ts:118](src/todo-app/src/features/reminders/reminder.store.ts#L118) |
| H4 | 🟠 Cao | `getTodoTitle()` trả Observable mới mỗi lần change detection → sub/unsub liên tục trong `@for` | [notification-bell.html:36](src/todo-app/src/features/reminders/notification-bell/notification-bell.html#L36) |
| H5 | 🟠 Cao | `environment.ts` (production) trỏ về `localhost:5100` | [environment.ts:3](src/todo-app/src/core/environments/environment.ts#L3) |
| H6 | 🟠 Cao | 4 package có lỗ hổng bảo mật đã công bố (AutoMapper, Microsoft.OpenApi, Snappier, SharpCompress) | Cả 2 csproj |
| H7 | 🟠 Cao | `reader.EndOfStream` trong method async (CA2024) — chặn thread, cản hủy kết nối SSE | [Bff/Stream/StreamEndpoint.cs:34](src/Todo.Bff/Features/Reminders/Stream/StreamEndpoint.cs#L34) |
| H8 | 🟠 Cao | `HttpClient` của Reminder đặt `Timeout.InfiniteTimeSpan` cho **mọi** request, không riêng stream | [Bff/Program.cs:16](src/Todo.Bff/Program.cs#L16) |
| M1 | 🟡 Trung bình | Không có authentication/authorization ở bất kỳ endpoint nào | Toàn bộ API/BFF |
| M2 | 🟡 Trung bình | `GET /api/todos` và `/api/reminders` không phân trang / không giới hạn | Các GetList handler |
| M3 | 🟡 Trung bình | Tham số `within` (upcoming) và `state` (reminders) bị bỏ qua / xử lý cứng | [GetUpcomingHandler.cs](src/Todo.Api/Features/Reminders/GetUpcoming/GetUpcomingHandler.cs), [GetRemindersHandler.cs](src/Todo.Api/Features/Reminders/GetList/GetRemindersHandler.cs) |
| M4 | 🟡 Trung bình | Dùng `Console.WriteLine` thay `ILogger` ở toàn bộ backend | 8 file |
| M5 | 🟡 Trung bình | `toggleAllTodos` bắn N request song song + double-flip UI, rollback lỗi không nhất quán | [todos.store.ts:321](src/todo-app/src/features/todos/todos.store.ts#L321) |
| M6 | 🟡 Trung bình | `route.paramMap.subscribe` không hủy đăng ký | [todo-list.ts:22](src/todo-app/src/features/todos/todo-list/todo-list.ts#L22) |
| M7 | 🟡 Trung bình | `ReminderProcessor` không `StopProcessingAsync`/dispose khi shutdown | [ReminderProcessor.cs](src/Todo.Api/Features/Reminders/ReminderProcessor.cs) |
| M8 | 🟡 Trung bình | Config đọc bằng `GetValue<string>` / `!` không kiểm tra null → NRE lúc khởi động nếu thiếu | [Api/Program.cs:17](src/Todo.Api/Program.cs#L17), [Bff/Program.cs:11](src/Todo.Bff/Program.cs#L11) |
| M9 | 🟡 Trung bình | Header SSE set bằng `Headers.Append` + `Connection: keep-alive` (hop-by-hop) | 2 StreamEndpoint |
| M10 | 🟡 Trung bình | 18 cảnh báo compiler chưa xử lý, chủ yếu CS8618 (nullable) | Phụ lục A |
| M11 | 🟡 Trung bình | Không có test nào; test mặc định `app.spec.ts` chắc chắn fail | [app.spec.ts:21](src/todo-app/src/app/app.spec.ts#L21) |
| M12 | 🟡 Trung bình | Thông báo lỗi hiển thị cho người dùng chứa tiếng lóng (`skibidi`, `LMAO`, `error code: 67`) | [CreateTodoCommandValidator.cs](src/Todo.Api/Features/Todos/Create/CreateTodoCommandValidator.cs) |
| L1 | 🔵 Thấp | Code chết: file bị `<Compile Remove>`, `TodoResponse`, `ReminderDto`/`ReminderState` ở BFF, `app.css` | Nhiều nơi |
| L2 | 🔵 Thấp | Khối comment code cũ khổng lồ (`TodosModule.cs` ~170 dòng comment) | Nhiều nơi |
| L3 | 🔵 Thấp | DTO nhân bản thủ công giữa API ↔ BFF ↔ Angular, dễ lệch | 3 tầng |
| L4 | 🔵 Thấp | `AllowedHosts: ""` ở API (khác chuẩn `"*"`) | [Api/appsettings.json](src/Todo.Api/appsettings.json) |
| L5 | 🔵 Thấp | Trùng route `DELETE /api/todos/completed` vs `/api/todos/{id}` | 2 endpoint |
| L6 | 🔵 Thấp | `RenderMode.Prerender` cho route có tham số `:filter` — nhiều khả năng chặn `ng build` | [app.routes.server.ts](src/todo-app/src/app/app.routes.server.ts) |
| L7 | 🔵 Thấp | `alert()` dùng làm kênh báo lỗi; nút icon-only thiếu `aria-label` | FE reminders |
| L8 | 🔵 Thấp | `ComponentStore` được `providedIn: 'root'` (dùng sai mục đích thiết kế) | 2 store |
| L9 | 🔵 Thấp | Chuỗi kết nối ASB nằm trong `appsettings.json` được commit | [Api/appsettings.json](src/Todo.Api/appsettings.json) |
| L10 | 🔵 Thấp | Solution `.slnx` không chứa frontend, không có project test | [Todo/Todo.slnx](Todo/Todo.slnx) |

---

## 3. Chi tiết — Mức Nghiêm trọng 🔴

### C1. `ReminderScanner` bị comment khỏi DI → chức năng Snooze chết một nửa

[src/Todo.Api/Program.cs:33](src/Todo.Api/Program.cs#L33)

```csharp
//builder.Services.AddHostedService<ReminderScanner>();
builder.Services.AddHostedService<ReminderProcessor>();
```

`ReminderScanner` là nơi **duy nhất** trong toàn bộ codebase đưa reminder từ `Snoozed` trở lại `Pending` khi `SnoozeUntil <= now`. Class vẫn tồn tại đầy đủ tại [ReminderScanner.cs](src/Todo.Api/Features/Reminders/ReminderScanner.cs) nhưng không bao giờ được chạy.

**Hệ quả**: người dùng bấm "Snooze 10m" → reminder chuyển sang `Snoozed` và **biến mất vĩnh viễn**. Sau 10 phút nó không quay lại chuông. Về mặt trải nghiệm, Snooze hiện đang hoạt động y hệt Dismiss.

### C2. Update `DueAt` không dọn `Reminder` cũ → reminder lần hai không bao giờ nổ

[UpdateTodoHandler.cs:37-52](src/Todo.Api/Features/Todos/Update/UpdateTodoHandler.cs#L37) hủy vé ASB cũ và đặt lịch mới đúng, nhưng **không xóa document `Reminder` đang tồn tại** của todo đó. Trong khi đó `ReminderProcessor` có chốt chặn chống trùng:

```csharp
// ReminderProcessor.cs:54
var alreadyExists = await DB.Find<Reminder>().Match(r => r.TodoId == todoId).ExecuteAnyAsync(...);
if (!alreadyExists) { /* chỉ tạo Reminder ở đây */ }
```

**Kịch bản lỗi**:
1. Tạo todo hạn 10:00 → tới hạn → `Reminder` được tạo → user Dismiss (state = `Dismissed`, document **vẫn còn** trong DB).
2. User sửa hạn thành 11:00 → ASB đặt lịch mới thành công.
3. 11:00 message nổ → processor thấy `alreadyExists == true` → bỏ qua → **không có thông báo nào**.

Đáng chú ý: [ToggleTodoHandler.cs:52](src/Todo.Api/Features/Todos/Toggle/ToggleTodoHandler.cs#L52) *có* xử lý đúng việc này (`DB.DeleteAsync<Reminder>`) — tức là logic đã được nhận diện ở một nhánh nhưng thiếu ở nhánh Update. Ngoài ra unique index trên `Reminder.TodoId` ([Program.cs](src/Todo.Api/Program.cs)) khiến document cũ tồn tại cũng chặn luôn mọi lần insert lại.

### C3. Không sửa được todo đã quá hạn

[UpdateTodoCommandValidator.cs:15](src/Todo.Api/Features/Todos/Update/UpdateTodoCommandValidator.cs#L15):

```csharp
RuleFor(x => x.DueAt).GreaterThan(DateTime.UtcNow).When(x => x.DueAt.HasValue)
```

Phía FE, khi double-click sửa todo, ô ngày được prefill bằng `dueAt` hiện tại và luôn được gửi kèm ([todo-item.ts:51](src/todo-app/src/features/todos/todo-item/todo-item.ts#L51)):

```typescript
dueAt: newDate ? newDate : undefined,
```

**Hệ quả**: với bất kỳ todo nào đã quá hạn, chỉ đổi mỗi tiêu đề cũng bị API trả 400 `"Due At can't be in the past"`. Store sau đó gọi `loadTodos()` để rollback → thay đổi của user bị nuốt mất kèm banner đỏ. Đây là kịch bản rất phổ biến (todo quá hạn nằm chình ình trong list là chuyện thường ngày).

### C4. Múi giờ không nhất quán trên toàn tuyến

Chuỗi xử lý hiện tại:

| Bước | Giá trị | Kind |
|---|---|---|
| Input `datetime-local` (FE) | `"2026-08-12T15:30"` — không timezone | — |
| `System.Text.Json` deserialize | `DateTime` | `Unspecified` |
| Validator so với `DateTime.UtcNow` | so sánh giờ **local** với giờ **UTC** | lệch 7h (UTC+7) |
| `ScheduleMessageAsync(msg, dueAt)` | `DateTime` Unspecified → `DateTimeOffset` dùng offset **máy chủ** | local |
| MongoDB lưu | serializer coi Unspecified là UTC | UTC |
| FE hiển thị `date: 'short'` | render lại theo local | **lệch +7h** |

Trong khi đó `CreateAt`, `CompletedAt`, `FiredAt`, `SnoozeUntil` đều dùng `DateTime.UtcNow` thuần.

**Hệ quả**: cùng một mốc thời gian được diễn giải khác nhau ở 3 nơi. Ngày hiển thị trên badge lệch với ngày user nhập; `GetUpcoming` (so `t.DueAt > now` với `now` là UTC) lọc sai cửa sổ 24h; validator "quá khứ/tương lai" cho kết quả sai trong khoảng lệch múi giờ.

### C5. Endpoint Statistics ném exception

[StatsOverviewHandler.cs:24](src/Todo.Api/Features/Statistics/StatsOverviewHandler.cs#L24) tính xong các mốc thời gian rồi `throw new NotImplementedException();`.

`ValidationExceptionHandler` chỉ bắt `ValidationException` (trả `false` cho loại khác), nên request `GET /api/stats/overview` rơi xuống ProblemDetails mặc định → **HTTP 500**. Endpoint này đã được `MapCarter` publish ra ngoài dù chưa có logic. BFF cũng chưa có slice tương ứng — thư mục [Todo.Bff/Features/Statistics/](src/Todo.Bff/) rỗng, chỉ tồn tại nhờ `<Folder Include>` trong csproj.

---

## 4. Chi tiết — Mức Cao 🟠

### H1. `DeleteCompleted` bỏ quên toàn bộ side-effect của Reminder

[DeleteCompletedTodosHandler.cs:14](src/Todo.Api/Features/Todos/DeleteCompleted/DeleteCompletedTodosHandler.cs#L14) chỉ có đúng một lệnh `DB.DeleteAsync<TodoItem>(...)`. Không `Publish` notification, không hủy lịch ASB, không đụng tới collection `Reminder`.

So sánh: `DeleteTodoHandler` **có** publish `TodoStateChangedNotification` để `TodoStateChangedHandler` dismiss reminder. Nhánh xóa hàng loạt thì không.

**Hệ quả**: reminder của các todo vừa bị xóa vẫn ở state `Pending` trong DB. SSE tiếp tục đẩy chúng về FE. Client có gọi `removeReminderByTodoId` cục bộ để che, nhưng chỉ cần F5 là chúng quay lại — kèm tiêu đề `"Đang tải (hoặc đã xóa)..."` vì `getTodoTitle` không tìm thấy todo tương ứng. Đây là rác dữ liệu vĩnh viễn, không có cơ chế nào dọn.

### H2. `DeleteTodo` không hủy Scheduled Message

[DeleteTodoHandler.cs](src/Todo.Api/Features/Todos/Delete/DeleteTodoHandler.cs) không inject `ReminderScheduler`, nên `ReminderSequenceNumber` của todo bị xóa không bao giờ được `CancelAsync`.

Message vẫn nằm trong queue tới đúng `DueAt` rồi mới nổ. Processor có xử lý êm (`todo is null` → skip + complete), nên không crash, nhưng queue tích lũy message chết theo thời gian. Trong 3 nhánh Create/Update/Toggle đều có gọi `CancelAsync`, riêng Delete thiếu — không nhất quán.

### H3. FE bóc lỗi validation sai cấu trúc → hiện `undefined`

Backend trả về mảng ([ValidationExceptionHandler.cs:33](src/Todo.Api/Common/Exceptions/ValidationExceptionHandler.cs#L33)):

```jsonc
"errors": [ { "field": "Minutes", "message": "Snooze minutes must be between 10 and 60" } ]
```

`todos.store.ts` đọc đúng dạng mảng (`beErrors[0].message`), nhưng `reminder.store.ts` lại đọc theo dạng dictionary kiểu ASP.NET cũ ([reminder.store.ts:118-119](src/todo-app/src/features/reminders/reminder.store.ts#L118)):

```typescript
const firstErrorKey = Object.keys(beErrors)[0];   // → "0"
errorMessage = beErrors[firstErrorKey][0];        // → ({field,message})[0] → undefined
```

**Hệ quả**: khi Snooze bị validator từ chối, user nhận đúng một hộp thoại `Can't Snooze: undefined`. Hai store trong cùng một app đang giả định hai contract lỗi khác nhau.

### H4. `getTodoTitle()` gọi trong template tạo Observable mới mỗi chu kỳ CD

[notification-bell.html:36](src/todo-app/src/features/reminders/notification-bell/notification-bell.html#L36):

```html
[title]="getTodoTitle(item.todoId) | async"
```

Method này (`notification-bell.ts:29`) `pipe()` ra một Observable **mới tinh** mỗi lần được gọi. Với change detection mặc định (không `OnPush`), mỗi chu kỳ CD × mỗi item trong `@for` sẽ tạo một Observable mới → `AsyncPipe` hủy sub cũ, sub mới liên tục.

Cùng file còn hai chỗ dùng `| async` lặp trên cùng một selector (`store.pendingCount$` ở dòng 19 và 20, `upcomingCount$` tương tự trong `reminder-panel.html`), nhân đôi subscription không cần thiết.

### H5. Environment production trỏ về localhost

[environment.ts:1-4](src/todo-app/src/core/environments/environment.ts#L1) — file production giống hệt file development:

```typescript
export const environment = { production: true, apiUrl: 'http://localhost:5100/bff' };
```

`angular.json` cấu hình `fileReplacements` thay `environment.ts` bằng `environment.development.ts` ở chế độ dev, tức bản **production** đang dùng chính file trỏ localhost. Bản build production sẽ không gọi được backend ở bất kỳ môi trường nào ngoài máy dev.

### H6. Bốn package có lỗ hổng bảo mật đã công bố

Từ output `dotnet build`:

| Package | Version | Mức | Advisory |
|---|---|---|---|
| `AutoMapper` | 14.0.0 | High | GHSA-rvv3-g6hj-g44x |
| `Microsoft.OpenApi` | 2.0.0 | High | GHSA-v5pm-xwqc-g5wc (cả 2 project) |
| `Snappier` | 1.0.0 | High | GHSA-pggp-6c3x-2xmx (transitive qua MongoDB) |
| `SharpCompress` | 0.30.1 | Moderate | GHSA-6c8g-7p36-r338 (transitive) |

Riêng `AutoMapper` là **dependency chết** — mọi Profile đã bị comment và `TodoMappings.cs`/`ReminderMapping.cs` bị loại khỏi build bằng `<Compile Remove>`, nhưng package vẫn được reference tại [Todo.Api.csproj:21](src/Todo.Api/Todo.Api.csproj#L21).

### H7. `reader.EndOfStream` trong method async (CA2024)

[Bff/Features/Reminders/Stream/StreamEndpoint.cs:34](src/Todo.Bff/Features/Reminders/Stream/StreamEndpoint.cs#L34) — compiler báo trực tiếp:

```
warning CA2024: Do not use 'reader.EndOfStream' in an async method
```

`EndOfStream` thực hiện đọc **đồng bộ** để dò EOF. Trên một SSE stream vô hạn, property này block thread pool cho tới khi API đẩy byte tiếp theo. Trong khoảng block đó `ct` không được quan sát, nên khi client đóng tab, vòng lặp không thoát ngay mà treo tới lần dữ liệu kế tiếp. Mỗi client SSE đang giữ một thread bị chặn.

### H8. Timeout vô hạn áp cho mọi call Reminder

[Bff/Program.cs:14-18](src/Todo.Bff/Program.cs#L14):

```csharp
builder.Services.AddHttpClient<ReminderApiClient>(c => {
    c.BaseAddress = ...;
    c.Timeout = Timeout.InfiniteTimeSpan; // vì client này còn dùng cho request stream dài vô hạn
});
```

Cùng một typed client phục vụ cả `GetReminderStreamAsync` (cần vô hạn) lẫn `GetPendingReminderAsync`, `GetUpcomingReminderAsync`, `SnoozeReminderAsync`, `DismissReminderAsync` (là request ngắn). Nếu `Todo.Api` treo, các request thường sẽ chờ mãi mãi thay vì fail nhanh — request của người dùng ở FE đứng hình vô thời hạn.

---

## 5. Chi tiết — Mức Trung bình 🟡

### M1. Không có xác thực / phân quyền
Mọi endpoint ở cả API lẫn BFF đều ẩn danh. BFF không thêm bất kỳ lớp bảo vệ nào — nó chỉ forward nguyên trạng, kể cả body và status code. Không có user context, nên dữ liệu todo là toàn cục dùng chung. Không có rate limiting, không có `UseHttpsRedirection`. Chấp nhận được với dự án training, nhưng cần ghi nhận nếu đưa lên môi trường thật.

### M2. Không phân trang
`GET /api/todos` với filter rỗng gọi `DB.Find<TodoItem>().ExecuteAsync()` — kéo **toàn bộ** collection về RAM. `GET /api/reminders?state=pending` tương tự. FE cũng luôn gọi `getTodos('all')` rồi lọc phía client ([todos.store.ts](src/todo-app/src/features/todos/todos.store.ts)) — nghĩa là tham số `filter` mà backend cất công xử lý gần như không bao giờ được dùng.

### M3. Tham số query bị bỏ qua hoặc xử lý cứng
- `GetUpcomingQuery.Within` được nhận, truyền xuống handler, rồi **không dùng** — cửa sổ 24h hardcode trong [GetUpcomingHandler.cs](src/Todo.Api/Features/Reminders/GetUpcoming/GetUpcomingHandler.cs).
- `GetRemindersHandler` chỉ hiểu `"pending"`; mọi giá trị khác (kể cả `"snoozed"`, `"all"`, hoặc rỗng) trả về mảng rỗng thay vì báo lỗi — sai lệch âm thầm, khó debug.
- So sánh dùng `ToLower()` phụ thuộc culture thay vì so sánh ordinal.

### M4. `Console.WriteLine` thay cho `ILogger`
Xuất hiện ở `CreateTodoHandler`, `UpdateTodoHandler`, `ReminderProcessor`, `ReminderScheduler`, `StreamEndpoint` (cả API và BFF). Không có log level, không có structured logging, không đi qua cấu hình `Logging` trong appsettings, không xuất hiện trong bất kỳ log sink nào khi deploy.

### M5. `toggleAllTodos` bắn N request rời rạc
[todos.store.ts:321-347](src/todo-app/src/features/todos/todos.store.ts#L321): vòng `forEach` gọi `this.toggleTodo(item.id)` cho từng item, sau đó gọi thêm `toggleInStore(isCompletedTarget)`.

Ba hệ quả: (1) list 100 item → 100 request PATCH; (2) mỗi `toggleTodo` đã tự lật UI trong `tap`, rồi `toggleInStore` lật lần nữa → double update; (3) nếu một request lỗi, nhánh rollback lật ngược đúng item đó trong khi `toggleInStore` đã set cả mảng → state hiển thị mâu thuẫn với server. Không có endpoint bulk-toggle ở backend.

### M6. Subscription route không được hủy
[todo-list.ts:22](src/todo-app/src/features/todos/todo-list/todo-list.ts#L22) gọi `this.route.paramMap.subscribe(...)` trong `ngOnInit` mà không `takeUntilDestroyed`/`unsubscribe`.

### M7. `ReminderProcessor` không dọn dẹp khi shutdown
[ReminderProcessor.cs](src/Todo.Api/Features/Reminders/ReminderProcessor.cs) `StartProcessingAsync` rồi `await Task.Delay(Timeout.Infinite, stoppingToken)`. Không override `StopAsync`, không `StopProcessingAsync`, không `DisposeAsync` processor. Khi app tắt, `Task.Delay` ném `TaskCanceledException`, message đang xử lý dở không được drain gọn. Ngoài ra `HandleErrorAsync` chỉ in message ra console và nuốt lỗi — message lỗi sẽ retry tới khi vào dead-letter mà không ai biết.

### M8. Config không được kiểm tra null
- [Api/Program.cs:17-19](src/Todo.Api/Program.cs#L17): `GetValue<string>("ConnectionStrings:MongoDB")` trả `string?` rồi truyền thẳng vào `MongoClientSettings.FromConnectionString` — thiếu config thì crash bằng NRE khó đọc.
- [Bff/Program.cs:11](src/Todo.Bff/Program.cs#L11): `builder.Configuration["TodoApi:BaseUrl"]!` — dùng null-forgiving để dập cảnh báo thay vì kiểm tra.
- `ServiceBusClient` được `new` thủ công rồi `AddSingleton(instance)`, nằm ngoài vòng đời DI quản lý.

### M9. Header SSE
Cả hai `StreamEndpoint` dùng `ctx.Response.Headers.Append("Content-Type", ...)`. `Append` có thể tạo header trùng thay vì ghi đè. `Connection: keep-alive` là header hop-by-hop do Kestrel tự quản lý, đặt tay không có tác dụng và không hợp lệ dưới HTTP/2. Ngoài ra không có heartbeat/comment line, nên proxy trung gian có thể cắt kết nối khi idle.

### M10. 18 cảnh báo compiler chưa xử lý
7 cảnh báo `CS8618` (thuộc tính non-nullable không được khởi tạo) trên `TodoItem.Title`, `Reminder.TodoId`, `TodoStateChangedNotification.TodoId`, và các DTO của BFF. Dự án đã bật `<Nullable>enable</Nullable>` nhưng model chưa tuân thủ, nên độ an toàn null của cả solution chỉ mang tính hình thức.

### M11. Không có test
Không tồn tại project test cho `Todo.Api` hay `Todo.Bff`. Frontend chỉ có [app.spec.ts](src/todo-app/src/app/app.spec.ts) mặc định của CLI, và test này **chắc chắn fail**: nó assert `h1` chứa `"Hello, todo-app"`, trong khi template thật render `"TodoMVC"` (nằm trong `todo-input.html`, không phải `app.html`).

### M12. Thông báo lỗi hiển thị cho người dùng chứa tiếng lóng
[CreateTodoCommandValidator.cs](src/Todo.Api/Features/Todos/Create/CreateTodoCommandValidator.cs):

```csharp
.NotEmpty().WithMessage("Title can't be empty, error code: 67")
.MaximumLength(200).WithMessage("Title must 200 char MAX, skibidi");
RuleFor(x => x.DueAt)...WithMessage("Due At can't be in the past LMAO");
```

Các chuỗi này đi thẳng qua ProblemDetails → store → banner đỏ trên UI. `UpdateTodoCommandValidator` cùng logic nhưng dùng câu chữ nghiêm túc → thông điệp không nhất quán giữa hai luồng.

---

## 6. Chi tiết — Mức Thấp 🔵

**L1 — Code chết.** 8 file bị loại khỏi build bằng `<Compile Remove>` trong [Todo.Api.csproj:10-17](src/Todo.Api/Todo.Api.csproj#L10) nhưng vẫn nằm trong repo. `Todo.Api/Common/TodoResponse.cs` không được tham chiếu ở đâu (đã grep toàn bộ solution). `ReminderDto` và `enum ReminderState` trong [Todo.Bff/Features/Reminders/ReminderDtos.cs](src/Todo.Bff/Features/Reminders/ReminderDtos.cs) hoàn toàn không được dùng (BFF chỉ forward JSON thô); chỉ `SnoozeReminderRequest` là còn sống. `src/app/app.css` rỗng và không được component nào khai báo.

**L2 — Comment code cũ.** `TodosModule.cs` (~170 dòng), `RemindersModule.cs` (~110 dòng), `BffTodosModule.cs`, `BffRemindersModule.cs` gần như 100% là code cũ bị comment. Thêm các khối cuối file ở `ToggleTodoHandler.cs`, `UpdateTodoHandler.cs`, `ReminderScanner.cs`, `Bff/Stream/StreamEndpoint.cs`, `todos.store.ts`. Git history đã giữ những phần này rồi.

**L3 — DTO nhân bản 3 tầng.** `TodoDto`/`CreateTodoRequest`/`UpdateTodoRequest` và `ReminderDto` được viết tay riêng ở API, BFF và Angular. Không có contract chung, không sinh client từ OpenAPI — đổi field ở API sẽ lặng lẽ lệch với hai tầng còn lại (chính là gốc rễ của H3).

**L4 — `AllowedHosts: ""`.** [Api/appsettings.json](src/Todo.Api/appsettings.json) đặt chuỗi rỗng thay vì `"*"` như template chuẩn (BFF thì dùng `"*"`). Host filtering fallback về `*` nên vô hại, nhưng thể hiện ý định không rõ ràng.

**L5 — Trùng route DELETE.** `/api/todos/completed` ([DeleteCompletedTodosEndpoint](src/Todo.Api/Features/Todos/DeleteCompleted/DeleteCompletedTodosEndpoint.cs)) và `/api/todos/{id}` ([DeleteTodoEndpoint](src/Todo.Api/Features/Todos/Delete/DeleteTodoEndpoint.cs)) cùng khớp URL `DELETE /api/todos/completed`. Routing của ASP.NET Core ưu tiên literal segment nên hiện tại chạy đúng, nhưng đây là sự trùng lặp ngầm, chỉ cần thêm một constraint là đổi hành vi.

**L6 — Prerender route có tham số.** [app.routes.server.ts](src/todo-app/src/app/app.routes.server.ts) khai `{ path: '**', renderMode: RenderMode.Prerender }` trong khi [app.routes.ts](src/todo-app/src/app/app.routes.ts) có route tham số `:filter`. Angular SSR yêu cầu `getPrerenderParams` cho route có tham số khi prerender. *Chưa kiểm chứng được vì `node_modules` chưa cài* — cần chạy `ng build` để xác nhận đây là lỗi build hay chỉ là cảnh báo.

**L7 — UX/Accessibility.** `alert()` được dùng làm kênh báo lỗi Snooze ([reminder.store.ts](src/todo-app/src/features/reminders/reminder.store.ts)), chặn luồng UI và không đồng bộ với cơ chế banner đỏ mà `todos.store` dùng. Nút chuông và nút lịch là icon-only, không có `aria-label`/`title`. Panel dropdown không đóng khi click ra ngoài, không xử lý phím `Escape`.

**L8 — `ComponentStore` ở scope root.** `TodosStore` và `ReminderStore` đều `@Injectable({ providedIn: 'root' })`. `ComponentStore` được thiết kế để gắn vòng đời với component; ở scope root nó thực chất trở thành một global store nhưng không có công cụ của NgRx Store (devtools, action log, time-travel).

**L9 — Chuỗi kết nối trong file commit.** [Api/appsettings.json](src/Todo.Api/appsettings.json) chứa `ConnectionStrings:ServiceBus` với `SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true`. Đây là giá trị placeholder của emulator (không phải secret thật), nhưng thói quen đặt chuỗi kết nối trực tiếp trong file được commit sẽ thành rủi ro thật khi lên môi trường khác.

**L10 — Solution không đầy đủ.** [Todo/Todo.slnx](Todo/Todo.slnx) chỉ gồm 2 project backend. Frontend không có trong solution, không có project test nào, không có CI config, không có README hướng dẫn chạy (chỉ có `README.md` mặc định của Angular CLI). Người mới vào phải tự suy ra thứ tự khởi động: Docker Mongo → ASB Emulator → Api → Bff → Angular.

---

## 7. Những điểm làm tốt

Để cân bằng, đây là các phần được làm chỉn chu:

1. **Vertical Slice + REPR nhất quán** — cấu trúc thư mục theo feature rất rõ, mỗi slice tự chứa Endpoint/Command/Handler/Validator/Response. Rất dễ tìm code theo tính năng.
2. **Chuyển từ polling sang event-driven đúng hướng** — thay `ReminderScanner` quét DB mỗi 30s bằng ASB Scheduled Message là một quyết định kiến trúc tốt; comment giải thích lý do được giữ lại ngay tại chỗ.
3. **SSE thay cho polling ở tầng BFF** — bản polling 10s cũ đã được thay bằng relay stream thật, `ReminderStreamChannel` cấp channel riêng cho từng connection và có `Unsubscribe` trong `finally` để chống rò subscriber.
4. **`ValidationBehavior` qua MediatR pipeline** — validation tập trung, endpoint không còn lặp lại `validator.ValidateAsync`.
5. **Update trực tiếp xuống DB** — dùng `DB.Update<T>().Modify(...)` và `DB.DeleteAsync<T>(predicate)` thay vì load-rồi-save, tiết kiệm round-trip. Có index cho `IsCompleted`/`DueAt` và unique index chống reminder trùng.
6. **Optimistic update + rollback ở FE** — `snoozeTodo` backup item trước khi xóa khỏi UI và khôi phục khi API lỗi; `toggleTodo` lật UI trước rồi mới gọi API. Chọn `concatMap` cho lệnh ghi và `switchMap` cho lệnh đọc là chính xác.
7. **Guard SSR** — `getStream()` kiểm tra `isPlatformBrowser` trước khi tạo `EventSource`, tránh nổ khi render phía server.
8. **`UpdateTodoHandler` chỉ đụng ASB khi `DueAt` thực sự đổi** — tránh cancel/reschedule vô ích khi user chỉ sửa tiêu đề.
9. **Comment giải thích "tại sao"** — dù văn phong suồng sã, phần lớn comment giải thích lý do lựa chọn (vì sao dùng `concatMap` chứ không `switchMap`, vì sao phải `Task.Delay(Infinite)`), đây là loại comment có giá trị nhất.

---

## Phụ lục A — Output build

```
dotnet build Todo/Todo.slnx
Build succeeded.  18 Warning(s)  0 Error(s)   (3.30s)
```

| Loại | Số lượng | Nội dung |
|---|---|---|
| NU1903 (High) | 3 | AutoMapper 14.0.0, Microsoft.OpenApi 2.0.0 (×2 project), Snappier 1.0.0 |
| NU1902 (Moderate) | 1 | SharpCompress 0.30.1 |
| CS8618 | 7 | `TodoItem.Title`, `Reminder.TodoId`, `TodoStateChangedNotification.TodoId`, `Bff/TodoDtos.Title` (×2), `Bff/ReminderDtos.Id`, `Bff/ReminderDtos.TodoId` |
| CA2024 | 1 | `Bff/Features/Reminders/Stream/StreamEndpoint.cs:34` — `EndOfStream` trong method async |

*(Các cảnh báo NU19xx được liệt kê lặp giữa giai đoạn restore và build nên tổng đếm là 18.)*

## Phụ lục B — Những gì chưa kiểm chứng được

Cần ghi rõ để tránh hiểu nhầm rằng toàn bộ báo cáo đều đã chạy thực tế:

- **Frontend chưa được build hay test**: `node_modules` không tồn tại trong workspace, nên không chạy được `ng build` / `ng test` / `tsc`. Mọi phát hiện phía Angular (H3, H4, H5, M5, M6, M11, L6, L7, L8) đến từ đọc code tĩnh. Riêng **L6** (prerender + route tham số) là phát hiện cần xác nhận bằng build thật.
- **Chưa chạy runtime**: không khởi động MongoDB, Azure Service Bus Emulator, API, BFF hay Angular. Các lỗi logic C1–C4, H1, H2 được suy ra bằng đọc luồng code và đối chiếu chéo giữa các handler, chưa reproduce trên môi trường chạy thật.
- **Chưa kiểm thử hiệu năng/tải**: các nhận định về M2 (không phân trang), H7 (block thread) là phân tích cấu trúc, không kèm số đo.

---

*Tài liệu này chỉ mô tả hiện trạng và tác động của các vấn đề, không bao gồm hướng dẫn hay prompt sửa lỗi, theo đúng yêu cầu.*
