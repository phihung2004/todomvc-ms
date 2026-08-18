# Ghi chú học tập: TodoMVC + Backend + BFF + Angular RxJS

Tài liệu này tổng hợp kiến thức thực tế từ repo này, nhằm mục đích ôn tập và ghi nhớ cho NotebookLM.

## 1. Tổng quan kiến trúc

### 1.1 Kiến trúc đang dùng

Repo này không phải là một microservice thật sự theo nghĩa tách riêng 3–4 service độc lập. Nó đang theo mô hình sau:

- Frontend: Angular SPA
- Backend API: ASP.NET Core + Carter + MediatR
- BFF (Backend for Frontend): một lớp trung gian đóng vai trò proxy để FE gọi tới API, đồng thời xử lý CORS và SSE stream
- Database: MongoDB (qua MongoDB.Entities)
- Messaging: Azure Service Bus

Nói ngắn gọn: đây là một dạng architecture layered + feature-based modular, có thêm yếu tố BFF và event-driven background workers.

Các lớp chính:

- [todo-app/src](todo-app/src): ứng dụng UI
- [Todo.Bff](Todo.Bff): lớp proxy / API gateway cho FE
- [Todo.Api](Todo.Api): miền nghiệp vụ + database + reminder processing
- [Todo.Api/Entities](Todo.Api/Entities): model nghiệp vụ
- [Todo.Api/Features](Todo.Api/Features): các feature tách theo từng domain

### 1.2 Cách BE và FE giao tiếp

Cấu trúc giao tiếp hiện tại:

1. FE Angular gọi vào BFF, không gọi thẳng tới Todo.Api.
2. BFF dùng HttpClient để forward request xuống Todo.Api.
3. BE trả JSON chuẩn; FE xử lý bằng Angular HttpClient và RxJS.
4. Với reminder realtime, dùng SSE (Server-Sent Events) thay vì WebSocket.

Ví dụ thực tế:

- [Todo.Bff/Program.cs](Todo.Bff/Program.cs): đăng ký typed HttpClient cho các resource
- [Todo.Bff/Features/Todos/TodoApiClient.cs](Todo.Bff/Features/Todos/TodoApiClient.cs): BFF gọi API Todo
- [Todo.Bff/Features/Reminders/Stream/StreamEndpoint.cs](Todo.Bff/Features/Reminders/Stream/StreamEndpoint.cs): BFF lắp SSE qua phía BE
- [todo-app/src/features/reminders/reminder-api.service.ts](todo-app/src/features/reminders/reminder-api.service.ts): FE mở EventSource tới /bff/reminders/stream

Như vậy, project đang dùng:

- REST cho CRUD và query
- SSE cho realtime notification
- Azure Service Bus cho delayed job / scheduled reminder

### 1.3 Sơ đồ luồng dữ liệu tổng quát

Mô tả text:

- Người dùng thao tác trên Angular UI.
- UI gọi tới TodoStore / ReminderStore.
- Store dùng RxJS effect để gọi HTTP API hoặc mở EventSource.
- BFF nhận request và forward tới Todo.Api.
- Todo.Api xử lý bằng MediatR handlers.
- Dữ liệu được lưu trong MongoDB.
- Với reminder, khi Todo có DueAt, BE tạo message lên Azure Service Bus bằng ReminderScheduler.
- ReminderProcessor đọc queue, tạo Reminder trong MongoDB và gửi delivery qua EmailReminderChannel.
- StreamEndpoint push reminder pending qua SSE tới FE.
- FE update UI ngay bằng state store.

---

## 2. Công nghệ & thư viện

### 2.1 BE – công nghệ chính

#### ASP.NET Core / Minimal API

- File chính: [Todo.Api/Program.cs](Todo.Api/Program.cs), [Todo.Bff/Program.cs](Todo.Bff/Program.cs)
- Dùng để khởi tạo app, đăng ký dependency injection, routing, CORS, exception handler.
- Vì sao chọn: phù hợp với kiến trúc nhỏ, nhanh, dễ triển khai, phù hợp với project demo/feature-based API.

Ví dụ:

```csharp
builder.Services.AddCarter();
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
```

Nguồn: [Todo.Api/Program.cs](Todo.Api/Program.cs)

#### Carter

- Dùng để định nghĩa endpoint theo module theo kiểu feature-oriented.
- File: [Todo.Api/Features/Todos/Create/CreateTodoEndpoint.cs](Todo.Api/Features/Todos/Create/CreateTodoEndpoint.cs), [Todo.Api/Features/Reminders/Stream/StreamEndpoint.cs](Todo.Api/Features/Reminders/Stream/StreamEndpoint.cs)
- Dùng để tách module theo feature: Todos, Reminders, Statistics.
- Vì sao chọn: code sạch hơn, endpoint không bị nhồi vào Program.cs.

Ví dụ:

```csharp
public class CreateTodoEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/todos", async (CreateTodoRequest request, IMediator mediator) =>
        {
            var command = new CreateTodoCommand(request.Title, request.DueAt);
            var response = await mediator.Send(command);
            return Results.Created($"/api/todos/{response.Id}", response);
        });
    }
}
```

Nguồn: [Todo.Api/Features/Todos/Create/CreateTodoEndpoint.cs](Todo.Api/Features/Todos/Create/CreateTodoEndpoint.cs)

#### MediatR

- Dùng để tách endpoint -> command/query -> handler.
- File: [Todo.Api/Features/Todos/GetList/GetListEndpoint.cs](Todo.Api/Features/Todos/GetList/GetListEndpoint.cs), [Todo.Api/Features/Todos/GetList/GetListHandler.cs](Todo.Api/Features/Todos/GetList/GetListHandler.cs)
- Dùng để thực hiện CQRS đơn giản: endpoint chỉ nhận request, send vào mediator, handler xử lý logic.
- Vì sao chọn: giảm coupling giữa API layer với business logic; dễ test và mở rộng feature.

Ví dụ:

```csharp
var query = new GetListQuery(filter);
var response = await mediator.Send(query);
```

Nguồn: [Todo.Api/Features/Todos/GetList/GetListEndpoint.cs](Todo.Api/Features/Todos/GetList/GetListEndpoint.cs)

#### FluentValidation

- Dùng để validate input và chuẩn hóa rule.
- File: [Todo.Api/Common/Behavior/ValidationBehavior.cs](Todo.Api/Common/Behavior/ValidationBehavior.cs), [Todo.Api/Common/Exceptions/ValidationExceptionHandler.cs](Todo.Api/Common/Exceptions/ValidationExceptionHandler.cs)
- Dùng như middleware pipeline cho MediatR.
- Vì sao chọn: không cần validate thủ công ở mỗi endpoint; validation chạy tập trung và đồng nhất.

Ví dụ:

```csharp
var validationFailures = await Task.WhenAll(
    _validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

if (errors.Any())
{
    throw new ValidationException(errors);
}
```

Nguồn: [Todo.Api/Common/Behavior/ValidationBehavior.cs](Todo.Api/Common/Behavior/ValidationBehavior.cs)

#### MongoDB.Entities

- Dùng trực tiếp với MongoDB, thay vì classic repository class.
- File: [Todo.Api/Entities/TodoItem.cs](Todo.Api/Entities/TodoItem.cs), [Todo.Api/Entities/Reminder.cs](Todo.Api/Entities/Reminder.cs)
- Dùng để model entity, save, query, update, index.
- Vì sao chọn: code đơn giản, trực quan, thao tác MongoDB nhanh cho demo.

Ví dụ:

```csharp
await DB.InitAsync("todo_mongo", settings);
await DB.Index<TodoItem>()
    .Key(t => t.IsCompleted, KeyType.Ascending)
    .Key(t => t.DueAt, KeyType.Ascending)
    .CreateAsync();
```

Nguồn: [Todo.Api/Program.cs](Todo.Api/Program.cs)

#### Azure Service Bus

- Dùng cho delayed job reminder.
- File: [Todo.Api/Features/Reminders/ReminderScheduler.cs](Todo.Api/Features/Reminders/ReminderScheduler.cs), [Todo.Api/Features/Reminders/ReminderProcessor.cs](Todo.Api/Features/Reminders/ReminderProcessor.cs)
- Dùng để schedule message theo DueAt và consumer xử lý sau đó.
- Vì sao chọn: đúng cho use case "nếu todo có due date thì queue một task, sau đó xử lý vào thời điểm cần nhắc".

Ví dụ:

```csharp
var message = new ServiceBusMessage(JsonSerializer.Serialize(payload));
return await _sender.ScheduleMessageAsync(message, dueAt, ct);
```

Nguồn: [Todo.Api/Features/Reminders/ReminderScheduler.cs](Todo.Api/Features/Reminders/ReminderScheduler.cs)

#### FluentEmail + SMTP

- Dùng để gửi email reminder.
- File: [Todo.Api/Program.cs](Todo.Api/Program.cs), [Todo.Api/Features/Reminders/Delivery/EmailReminderChannel.cs](Todo.Api/Features/Reminders/Delivery/EmailReminderChannel.cs)
- Dùng ở Delivery layer để gửi thông báo khi reminder tới hạn.
- Vì sao chọn: dễ mở rộng channel khác nếu muốn add SMS hoặc Push.

#### BackgroundService

- Dùng cho scanner và processor.
- File: [Todo.Api/Features/Reminders/ReminderScanner.cs](Todo.Api/Features/Reminders/ReminderScanner.cs), [Todo.Api/Features/Reminders/ReminderProcessor.cs](Todo.Api/Features/Reminders/ReminderProcessor.cs)
- Scanner chuyển reminder snoozed → pending khi hết thời gian; Processor xử lý queue message.
- Vì sao chọn: worker chạy nền không cần HTTP request.

### 2.2 FE – công nghệ chính

#### Angular 20 + standalone component

- File: [todo-app/src/app/app.config.ts](todo-app/src/app/app.config.ts), [todo-app/src/features/todos/todo-list/todo-list.ts](todo-app/src/features/todos/todo-list/todo-list.ts)
- Dùng để render UI, routing, service injection.
- Vì sao chọn: mạnh cho SPA, dùng signal + standalone component hiện đại, phù hợp với project demo.

#### RxJS

- File: [todo-app/src/features/todos/todos.store.ts](todo-app/src/features/todos/todos.store.ts), [todo-app/src/features/reminders/reminder.store.ts](todo-app/src/features/reminders/reminder.store.ts)
- Dùng cho effect, stream, tự động refresh, SSE, optimistic update.
- Vì sao chọn: Angular và state management tốt nhất khi xử lý async stream.

#### @ngrx/component-store

- File: [todo-app/src/features/todos/todos.store.ts](todo-app/src/features/todos/todos.store.ts), [todo-app/src/features/reminders/reminder.store.ts](todo-app/src/features/reminders/reminder.store.ts), [todo-app/src/features/statistics/stats.store.ts](todo-app/src/features/statistics/stats.store.ts)
- Dùng để quản lý state của feature: todos, reminder, stats.
- Vì sao chọn: lightweight hơn Redux, rất hợp cho feature đơn lẻ trong app không quá lớn.

#### HttpClient

- File: [todo-app/src/features/todos/todo-api.service.ts](todo-app/src/features/todos/todo-api.service.ts), [todo-app/src/features/statistics/stats-api.service.ts](todo-app/src/features/statistics/stats-api.service.ts)
- Dùng để gọi REST endpoint.
- Vì sao chọn: gọi API chuẩn của Angular; tích hợp tốt với RxJS observable.

#### EventSource (SSE)

- File: [todo-app/src/features/reminders/reminder-api.service.ts](todo-app/src/features/reminders/reminder-api.service.ts)
- Dùng để mở stream realtime từ BFF.
- Vì sao chọn: đơn giản hơn WebSocket cho use case push notification light-weight.

#### Angular Router

- File: [todo-app/src/app/app.routes.ts](todo-app/src/app/app.routes.ts)
- Dùng để điều hướng /all, /active, /completed, /stats.
- Vì sao chọn: router là chuẩn của Angular, phù hợp với SPA.

---

## 3. Các pattern & kỹ thuật code đáng học

### 3.1 CQRS + MediatR

Đây là pattern rõ nhất ở BE.

- Endpoint làm nhiệm vụ network boundary.
- Handler làm business logic.
- Query/Command là contract rõ ràng.

Ví dụ:

- [Todo.Api/Features/Todos/Create/CreateTodoCommand.cs](Todo.Api/Features/Todos/Create/CreateTodoCommand.cs)
- [Todo.Api/Features/Todos/Create/CreateTodoHandler.cs](Todo.Api/Features/Todos/Create/CreateTodoHandler.cs)
- [Todo.Api/Features/Todos/Create/CreateTodoEndpoint.cs](Todo.Api/Features/Todos/Create/CreateTodoEndpoint.cs)

Ưu điểm:

- Một request đi đúng một “đường” rõ ràng.
- Dễ thêm validation, tracing, retry.

### 3.2 Middleware pattern / Pipeline behavior

ValidationBehavior là một dạng middleware cho MediatR pipeline.

- File: [Todo.Api/Common/Behavior/ValidationBehavior.cs](Todo.Api/Common/Behavior/ValidationBehavior.cs)

Logic:

- request đi qua toàn bộ validators
- nếu có lỗi thì throw ValidationException
- exception handler sẽ trả ProblemDetails cho client

Đây là mẫu rất hay để chuẩn hóa validation trên toàn hệ thống.

### 3.3 Observer / Event-driven pattern

Repo có nhiều dấu hiệu observer/event-driven:

- [Todo.Api/Common/TodoStateChangedNotification.cs](Todo.Api/Common/TodoStateChangedNotification.cs)
- [Todo.Api/Features/Reminders/TodoStateChangedHandler.cs](Todo.Api/Features/Reminders/TodoStateChangedHandler.cs)
- [Todo.Api/Features/Reminders/Messaging/ReminderStreamChannel.cs](Todo.Api/Features/Reminders/Messaging/ReminderStreamChannel.cs)

Logic:

- Khi todo bị xóa hoặc toggle, BE publish notification.
- Handler sẽ dọn reminder liên quan.
- StreamChannel giúp nhiều subscriber nhận sự kiện mới reminder.

### 3.4 Strategy pattern (channel delivery)

- Interface: [Todo.Api/Features/Reminders/Delivery/IReminderDeliveryChannel.cs](Todo.Api/Features/Reminders/Delivery/IReminderDeliveryChannel.cs)
- Implementation: [Todo.Api/Features/Reminders/Delivery/EmailReminderChannel.cs](Todo.Api/Features/Reminders/Delivery/EmailReminderChannel.cs)

Điểm hay:

- Service delivery không biết phía sau dùng email hay SMS. Nó chỉ gọi interface.
- Dễ mở rộng thêm channel mới như Push, SMS.

### 3.5 Background worker / polling + scheduled task

- [Todo.Api/Features/Reminders/ReminderScanner.cs](Todo.Api/Features/Reminders/ReminderScanner.cs)
- [Todo.Api/Features/Reminders/ReminderProcessor.cs](Todo.Api/Features/Reminders/ReminderProcessor.cs)

Phần này rất đáng học vì cho thấy:

- worker nền chạy mãi
- polling hoặc queue-based processing
- xử lý lịch trình khác với xử lý API trực tiếp

### 3.6 ComponentStore + selectors + effects

Đây là pattern chính ở frontend.

- [todo-app/src/features/todos/todos.store.ts](todo-app/src/features/todos/todos.store.ts)
- [todo-app/src/features/reminders/reminder.store.ts](todo-app/src/features/reminders/reminder.store.ts)
- [todo-app/src/features/statistics/stats.store.ts](todo-app/src/features/statistics/stats.store.ts)

Store gồm:

- state
- selector
- updater
- effect

Ví dụ:

```ts
readonly loadTodos = this.effect<void>((trigger$) =>
  trigger$.pipe(
    tap(() => this.setLoading(true)),
    switchMap(() =>
      this.todoApiService.getTodos('all').pipe(
        tapResponse(
          (response) => this.setTodos(response),
          (error) => this.setError("Can't load TodoList")
        )
      )
    )
  )
);
```

Đây là mẫu rất tốt để học cách “stream state + async effect + UI update”.

### 3.7 Optimistic UI

- Dùng trong [todo-app/src/features/todos/todos.store.ts](todo-app/src/features/todos/todos.store.ts) và [todo-app/src/features/reminders/reminder.store.ts](todo-app/src/features/reminders/reminder.store.ts)

Ví dụ:

```ts
tap((id) => this.toggleSingleTodoInStore(id)),
concatMap((id) => this.todoApiService.toggleTodo(id).pipe(...))
```

Ý nghĩa:

- UI đổi ngay khi click
- API chạy ở background
- nếu lỗi thì rollback lại state

Đây là kỹ thuật cực quan trọng trong UX hiện đại.

### 3.8 Validation & exception handling chuẩn

- BE: [Todo.Api/Common/Exceptions/ValidationExceptionHandler.cs](Todo.Api/Common/Exceptions/ValidationExceptionHandler.cs)
- FE: parse `error.error.errors` trong [todo-app/src/features/todos/todos.store.ts](todo-app/src/features/todos/todos.store.ts)

Ví dụ:

```ts
const beErrors = error.error?.errors;
if (beErrors && beErrors.length > 0) {
  const errorMessage = beErrors[0].Message || beErrors[0].message;
  this.setError(errorMessage);
}
```

Điểm hay: đã có chuẩn `ProblemDetails` và client parse lên UI.

### 3.9 Caching / auto-refresh / polling

- [todo-app/src/features/statistics/stats.store.ts](todo-app/src/features/statistics/stats.store.ts): `timer(0, 30000)`
- [todo-app/src/features/reminders/reminder.store.ts](todo-app/src/features/reminders/reminder.store.ts): `connectStream()` dùng SSE

Tức là project đã có 2 kiểu “refresh”:

- polling/auto refresh cho stats
- stream push cho reminder

### 3.10 Dạng “module-based architecture”

Mỗi feature có folder riêng và thường có:

- endpoint
- handler
- query/command
- dto
- validator

Ví dụ:

- [Todo.Api/Features/Todos](Todo.Api/Features/Todos)
- [Todo.Api/Features/Reminders](Todo.Api/Features/Reminders)
- [Todo.Api/Features/Statistics](Todo.Api/Features/Statistics)

Đây là best practice rất phù hợp cho scale-up feature.

> Lưu ý: repo này không thấy Factory pattern rõ ràng, không thấy HOC pattern trong Angular, cũng không thấy custom hook pattern rõ ràng. Nhưng thấy rõ CQRS, pipeline middleware, observer, strategy, optimistic update, state store.

---

## 4. SOLID & Clean Code trong project

### 4.1 S – Single Responsibility Principle

Tốt ở nhiều chỗ:

- Endpoint chỉ nhận request và chuyển tới mediator.
- Handler chỉ xử lý business logic.
- Service Scheduler chỉ làm schedule message.
- Delivery channel chỉ làm gửi mail.

Ví dụ:

- [Todo.Api/Features/Todos/Create/CreateTodoEndpoint.cs](Todo.Api/Features/Todos/Create/CreateTodoEndpoint.cs)
- [Todo.Api/Features/Reminders/ReminderScheduler.cs](Todo.Api/Features/Reminders/ReminderScheduler.cs)
- [Todo.Api/Features/Reminders/Delivery/EmailReminderChannel.cs](Todo.Api/Features/Reminders/Delivery/EmailReminderChannel.cs)

Điểm cần cải thiện:

- Các handler như [Todo.Api/Features/Todos/Toggle/ToggleTodoHandler.cs](Todo.Api/Features/Todos/Toggle/ToggleTodoHandler.cs) đang làm đồng thời: update DB, cancel schedule, re-schedule, publish event. Muốn thật clean hơn, nên tách ra thành các service như `TodoReminderLifecycleService` hoặc `TodoStateCoordinatorService`.

### 4.2 O – Open/Closed Principle

Tốt ở chỗ delivery channel mở rộng dễ dàng.

- Interface: [Todo.Api/Features/Reminders/Delivery/IReminderDeliveryChannel.cs](Todo.Api/Features/Reminders/Delivery/IReminderDeliveryChannel.cs)
- Service sử dụng `IEnumerable<IReminderDeliveryChannel>`: [Todo.Api/Features/Reminders/Delivery/ReminderDeliveryService.cs](Todo.Api/Features/Reminders/Delivery/ReminderDeliveryService.cs)

Nếu muốn thêm SMS channel, chỉ cần tạo class mới implements interface mà không sửa service cũ.

### 4.3 L – Liskov Substitution Principle

Repo có dấu hiệu tuân thủ tốt vì các channel đều implement cùng interface.

Ví dụ:

```csharp
public interface IReminderDeliveryChannel
{
    string ChannelName { get; }
    Task<bool> SendAsync(Reminder reminder, TodoItem todo, CancellationToken ct);
}
```

Mọi class như EmailReminderChannel đều có thể thay thế cho nhau trong `ReminderDeliveryService`.

### 4.4 I – Interface Segregation Principle

Tốt:

- Interface nhỏ, mỗi channel chỉ cần method `SendAsync` và property `ChannelName`.
- Store/Service không cần phụ thuộc các method không dùng.

### 4.5 D – Dependency Inversion Principle

Tốt ở chỗ:

- Handler inject `ReminderScheduler` thay vì `new ReminderScheduler(...)`.
- `ReminderDeliveryService` dùng interface `IReminderDeliveryChannel` thay vì phụ thuộc trực tiếp Email.

Ví dụ:

```csharp
private readonly IEnumerable<IReminderDeliveryChannel> _channels;
public ReminderDeliveryService(
    IEnumerable<IReminderDeliveryChannel> channels,
    ILogger<ReminderDeliveryService> logger)
```

Nguồn: [Todo.Api/Features/Reminders/Delivery/ReminderDeliveryService.cs](Todo.Api/Features/Reminders/Delivery/ReminderDeliveryService.cs)

### 4.6 Clean Code nhìn nhận

Điểm mạnh:

- tên file và class tổ chức theo feature
- tên method rõ ràng: `ScheduleAsync`, `CancelAsync`, `connectStream`, `loadTodos`
- dùng `record` cho command/query rất clean

Điểm yếu:

- nhiều đoạn code đã comment cũ, logic thử nghiệm còn tồn tại trong các file [Todo.Api/Features/Todos/TodosModule.cs](Todo.Api/Features/Todos/TodosModule.cs) và [Todo.Api/Features/Reminders/RemindersModule.cs](Todo.Api/Features/Reminders/RemindersModule.cs)
- kết hợp quá nhiều trách nhiệm trong cùng handler (`UpdateTodoHandler`, `ToggleTodoHandler`)
- có một số logic “fix bug” được ghi trong comments, thể hiện project đang trong trạng thái iterative / prototype

### 4.7 Gợi ý refactor nếu muốn nâng cấp

- tách `ReminderLifecycleService` từ `ToggleTodoHandler` và `UpdateTodoHandler`
- tách `TodoReminderScheduler` khỏi business handler
- tạo `TodoValidationService` hoặc `FluentValidation` rule riêng cho mỗi request
- bổ sung unit tests cho handler và service

---

## 5. Best Practices rút ra được

### 5.1 Naming convention

Project có naming khá rõ và dễ đọc:

- `CreateTodoCommand`, `GetListQuery`, `ToggleTodoHandler`
- `TodoApiService`, `ReminderStore`, `StatsStore`
- `loadTodos`, `connectStream`, `setPendingReminders`

Điểm này rất tốt vì nó tuân theo domain-driven naming và action-oriented naming.

### 5.2 Cấu trúc thư mục

- BE: feature folder theo domain
- FE: feature folder theo tính năng, không nhồi toàn bộ vào `src/app`

Ví dụ:

- [Todo.Api/Features/Todos](Todo.Api/Features/Todos)
- [Todo.Api/Features/Reminders](Todo.Api/Features/Reminders)
- [todo-app/src/features/todos](todo-app/src/features/todos)
- [todo-app/src/features/reminders](todo-app/src/features/reminders)

Điểm hay: dễ mở rộng thêm feature mới mà không làm rối.

### 5.3 Cách viết API

- dùng endpoint rõ ràng: `/api/todos`, `/api/reminders`, `/api/stats/overview`
- dùng `Results.Ok`, `Results.Created`, `Results.Problem`
- dùng `IMediator` để tách request khỏi handler

Ví dụ:

```csharp
return Results.Created($"/api/todos/{response.Id}", response);
```

Nguồn: [Todo.Api/Features/Todos/Create/CreateTodoEndpoint.cs](Todo.Api/Features/Todos/Create/CreateTodoEndpoint.cs)

### 5.4 Cách xử lý exception

- Validation failure -> custom `IExceptionHandler`
- ProblemDetails -> chuẩn JSON error cho FE

Ví dụ:

```csharp
var problemDetails = new ProblemDetails
{
    Status = StatusCodes.Status400BadRequest,
    Title = "Validation Failed",
    Detail = "One or more validation errors occurred."
};
```

Nguồn: [Todo.Api/Common/Exceptions/ValidationExceptionHandler.cs](Todo.Api/Common/Exceptions/ValidationExceptionHandler.cs)

### 5.5 Cách viết test

Điểm yếu lớn nhất: repo này chưa có test nghiệp vụ đầy đủ cho backend.

- Angular có spec mặc định: [todo-app/src/app/app.spec.ts](todo-app/src/app/app.spec.ts)
- Nhưng chưa có unit test cho handler, reminder logic, stream, validation, service bus schedule.

Nên học thêm:

- xUnit / NUnit cho C#
- Angular TestBed / Jasmine cho component và service
- integration tests cho API / BFF

### 5.6 Những điểm hay nên học theo

- tách feature rõ ràng
- dùng MediatR + handler cho mỗi thao tác
- dùng `ProblemDetails` để client biết lỗi gì
- dùng SSE cho realtime
- dùng `ComponentStore` thay vì chồng state trong component
- dùng optimistic update để tăng responsiveness

### 5.7 Những điểm dở nên tránh

- có code comment “cũ” và dead code còn nằm lại; dễ gây hiểu nhầm
- một số handler làm quá nhiều việc (database + scheduling + event publish)
- BFF có API client khá “thin”, nhưng không có DTO mapping/abstraction rõ ràng nên dễ thiếu tiêu chuẩn hóa
- FE có một số đoạn error parsing dạng hard-coded string, cần chuẩn hóa theo một `ErrorMapper`
- thiếu auth/authorization rõ ràng; project hiện chỉ là demo, nhưng nếu lên production phải thêm user identity và policy

---

## 6. Danh sách kiến thức nên học thêm

### 6.1 MediatR pipeline và middleware

- Vì sao quan trọng: là backbone của kiến trúc BE trong project; giúp tách request và business logic.

### 6.2 Azure Service Bus + scheduled messages

- Vì sao quan trọng: project đã dùng message queue để định thời lời nhắc; đây là kỹ thuật rất hay khi làm hệ thống thực tế.

### 6.3 SSE (Server-Sent Events)

- Vì sao quan trọng: FE và BE đang dùng chuẩn live update nhẹ, tốt cho notification nhưng không cần WebSocket đầy đủ.

### 6.4 RxJS nâng cao

- `switchMap`, `concatMap`, `mergeMap`, `withLatestFrom`, `tapResponse`, `combineLatest`
- Vì sao quan trọng: đây là “chìa khóa” để hiểu toàn bộ FE logic trong repo.

### 6.5 Angular ComponentStore

- Vì sao quan trọng: project dùng store thay vì component state; nếu không hiểu `effect`, `updater`, `select` sẽ khó debug.

### 6.6 MongoDB indexing và aggregation

- Vì sao quan trọng: project đã dùng `Index`, filter, aggregation cho stats và reminder queries; performance cần index tốt.

### 6.7 ProblemDetails + API contract chuẩn

- Vì sao quan trọng: FE parse lỗi từ `error.error.errors` và `ProblemDetails`; đây là kỹ thuật chuẩn trong REST API hiện đại.

### 6.8 Clean Architecture / layered structure

- Vì sao quan trọng: project đã có dạng modular feature, nhưng chưa hoàn toàn clean; nếu tiếp tục mở rộng thì cần tách domain services rõ hơn.

### 6.9 Auth & authorization

- Vì sao quan trọng: hiện project chưa có user identity; khi lên production cần có role-based access, JWT hoặc BFF security.

### 6.10 Testing strategy

- Vì sao quan trọng: kinh nghiệm tăng tốc development và giảm regressions; hiện repo đang thiếu test thực tế cho business logic.

---

## Tóm tắt nhanh

- Kiến trúc hiện tại: layered + feature-based + BFF + Angular SPA.
- FE gọi BFF, BFF gọi Todo.Api, BE dùng MediatR + Carter + MongoDB + Service Bus.
- RxJS là cốt lõi: `switchMap`, `concatMap`, `mergeMap`, `combineLatest`, `tapResponse` được dùng rất mạnh.
- `ComponentStore` giúp quản lý state cho todos, reminders, stats.
- BE có nhiều mẫu hay: CQRS, pipeline validation, event-driven notification, strategy pattern, background worker.
- Ưu điểm: cấu trúc module rõ, edge-case xử lý tốt, realtime và reminder logic khá rõ.
- Nhược điểm: code có nhiều dead comment, handler còn quá đa trách nhiệm, thiếu test nghiệp vụ và auth.
- Nếu muốn tiếp tục nâng cấp: tách service rõ hơn, chuẩn hóa error, thêm unit/integration test, chuẩn bị auth + authorization.

---

Nguồn chính tham khảo:

- [Todo.Api/Program.cs](Todo.Api/Program.cs)
- [Todo.Bff/Program.cs](Todo.Bff/Program.cs)
- [todo-app/package.json](todo-app/package.json)
- [todo-app/src/app/app.routes.ts](todo-app/src/app/app.routes.ts)
- [todo-app/src/features/todos/todos.store.ts](todo-app/src/features/todos/todos.store.ts)
- [todo-app/src/features/reminders/reminder.store.ts](todo-app/src/features/reminders/reminder.store.ts)
- [Todo.Api/Features/Reminders/ReminderProcessor.cs](Todo.Api/Features/Reminders/ReminderProcessor.cs)
- [Todo.Api/Common/Behavior/ValidationBehavior.cs](Todo.Api/Common/Behavior/ValidationBehavior.cs)
- [Todo.Api/Common/Exceptions/ValidationExceptionHandler.cs](Todo.Api/Common/Exceptions/ValidationExceptionHandler.cs)
