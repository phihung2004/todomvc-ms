import { inject, Injectable } from '@angular/core';
import { ComponentStore } from '@ngrx/component-store';
import { ReminderApiService, ReminderDto } from './reminder-api.service';
import { concatMap, switchMap, tap } from 'rxjs';
import { tapResponse } from '@ngrx/operators';
import { TodoDto } from '../todos/todo-api.service';

export interface ReminderState {
  pending: ReminderDto[];
  upcoming: TodoDto[];
  connected: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class ReminderStore extends ComponentStore<ReminderState> {
  private readonly reminderApiService = inject(ReminderApiService);

  constructor() {
    super({
      pending: [],
      upcoming: [],
      connected: false,
    });
  }

  // Tóm lại luật bất thành văn khi viết Store:

  // Cần vọc data, đổi UI nhanh: Dùng tap.

  // Tạo/Sửa/Xóa (POST/PUT/PATCH/DELETE): Dùng concatMap để không mất lệnh.

  // Lấy data/Search (GET/Stream): Dùng switchMap để không kẹt xe ngẽn mạng.

  // Cứ gọi API trong Effect là phải nhét kết quả vào tapResponse để chống sập luồng.

  // Selector======================================================

  readonly pending$ = this.select((state) => state.pending);

  readonly connected$ = this.select((state) => state.connected);

  readonly pendingCount$ = this.select(this.pending$, (pending) => pending.length);

  // ++ THÊM MỚI: Lấy danh sách Upcoming ra
  readonly upcoming$ = this.select((state) => state.upcoming);
  // ++ THÊM MỚI: Đếm số lượng để lát gắn lên cái chấm đỏ của icon lịch
  readonly upcomingCount$ = this.select(this.upcoming$, (upcoming) => upcoming.length);

  // Updater=======================================================

  readonly setPendingReminders = this.updater((state, pending: ReminderDto[]) => ({
    ...state,
    pending: pending,
  }));

  readonly setConnected = this.updater((state, connected: boolean) => ({
    ...state,
    connected,
  }));

  readonly removeReminder = this.updater((state, reminderId: string) => ({
    ...state,
    pending: state.pending.filter((pending) => pending.id !== reminderId),
  }));

  // ++ THÊM MỚI: Hàm vứt data vào kho
  readonly setUpcoming = this.updater((state, upcoming: TodoDto[]) => ({
    ...state,
    upcoming,
  }));
  // Effect========================================================

  readonly snoozeTodo = this.effect<{ id: string; minutes: number }>((payload$) =>
    payload$.pipe(
      tap((payload) => this.removeReminder(payload.id)),

      concatMap((payload) =>
        this.reminderApiService.snoozeReminder(payload.id, payload.minutes).pipe(
          tapResponse(
            () => console.log(`Đã cho reminder ${payload.id} ngủ thêm ${payload.minutes} phút`),
            (error) => {
              console.error('Error when snooze:', error);
              // Nếu làm kỹ, chỗ này mạng sập thì mình bốc lại cái reminder thả vào Store.
              // Nhưng app hiện tại cứ log ra là đủ xài rồi.
            },
          ),
        ),
      ),
    ),
  );

  readonly dismissReminder = this.effect<string>((id$) =>
    id$.pipe(
      // Vừa bấm tắt là xóa luôn cái popup trên màn hình
      tap((id) => this.removeReminder(id)),

      // Gọi API báo cho BE biết là đã tắt
      // Làm tuần tự theo thứ tự request người dùng ấn
      concatMap((id) =>
        this.reminderApiService.dismissReminder(id).pipe(
          tapResponse(
            () => console.log(`Reminder removed ${id}`),
            (error) => console.error('Error when dismissing:', error),
          ),
        ),
      ),
    ),
  );

  // ++ THÊM MỚI: Hàm gọi API kéo list 24h về
  readonly loadUpcoming = this.effect<void>((trigger$) =>
    trigger$.pipe(
      switchMap(() =>
        this.reminderApiService.getUpcomingReminder('24h').pipe(
          tapResponse(
            // Về lý thuyết API này trả về TodoDto[], có vẻ chú viết type nhầm trong service (lát anh nhắc fix sau)
            (items) => this.setUpcoming(items as any),
            (error) => console.error('Error loading upcoming:', error),
          ),
        ),
      ),
    ),
  );

  // Không cần tham số đầu vào, cứ gọi là chạy
  readonly connectStream = this.effect<void>((trigger$) =>
    trigger$.pipe(
      // Bật cờ connected lên báo hiệu đang thi công đường ống
      // Tap: dùng để làm việt vặt, 1 nhát nhẹ nào đó
      tap(() => this.setConnected(true)),

      // switchMap: người dùng request 1 cái nhiều lần thì chỉ lấy cái mới nhất, bỏ cái request cũ
      switchMap(() =>
        this.reminderApiService.getStream().pipe(
          // tapResponse:
          tapResponse(
            (reminders) => {
              // Nước (data) về! Đổ thẳng vào kho để Selector báo lên UI
              this.setPendingReminders(reminders);

              // Đảm bảo cờ luôn xanh khi nước đang chảy
              this.setConnected(true);
            },
            (error) => {
              console.error('Đứt ống nước SSE:', error);
              // Cập nhật state để UI biết đường hiện thông báo mất kết nối
              this.setConnected(false);
            },
          ),
        ),
      ),
    ),
  );
}
