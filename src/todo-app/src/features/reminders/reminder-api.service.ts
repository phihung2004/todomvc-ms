import { inject, Injectable, NgZone, PLATFORM_ID } from '@angular/core';
import { environment } from '../../core/environments/environment';
import { HttpClient } from '@angular/common/http';
import { EMPTY, Observable } from 'rxjs';
import { isPlatformBrowser } from '@angular/common';
import { TodoDto } from '../todos/todo-api.service';

export interface ReminderDto {
  id: string;
  todoId: string;
  dueAt: string | Date;
  state: ReminderState;
  snoozeUntil?: string | Date;
  firedAt: string | Date;
}

export enum ReminderState {
  // không mặc định số cũng được, chỉ là làm cho chắc
  Pending = 0,
  Snoozed = 1,
  Dismissed = 2,
}

export interface SnoozeReminderRequest {
  minutes: number; // vãi,không có int, gộp lại thành number hết r :)))
}

@Injectable({
  providedIn: 'root',
})
export class ReminderApiService {
  private readonly apiUrl = environment.apiUrl + '/reminders';

  private http = inject(HttpClient);

  private zone = inject(NgZone);

  // 1. Xin Angular cho biết mình đang chạy ở đâu (Browser hay Server)
  private platformId = inject(PLATFORM_ID);

  // wtf,đem cái ?state về BE như nào
  // À... chuỗi lội suy :))
  getPendingReminder(state: string): Observable<ReminderDto[]> {
    return this.http.get<ReminderDto[]>(`${this.apiUrl}?state=${state}`);
  }

  getUpcomingReminder(within: string): Observable<TodoDto[]> {
    return this.http.get<TodoDto[]>(`${this.apiUrl}/upcoming?within=${within}`);
  }

  snoozeReminder(id: string, minutes: number): Observable<void> {
    const request: SnoozeReminderRequest = { minutes };
    return this.http.patch<void>(`${this.apiUrl}/${id}/snooze`, request);
  }

  dismissReminder(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/dismiss`, null);
  }

  getStream(): Observable<ReminderDto[]> {
    if (!isPlatformBrowser(this.platformId)) {
      // Trả về một luồng rỗng (EMPTY), không làm gì cả, không quăng lỗi
      return EMPTY;
    }

    return new Observable<ReminderDto[]>((observer) => {
      // Mở van nước từ BFF
      const eventSource = new EventSource(`${this.apiUrl}/stream`);

      // Bắt sự kiện mỗi khi BFF bơm nước (data) xuống
      eventSource.onmessage = (event) => {
        // EventSource chạy ngầm ở ngoài, nên phải bế nó vào trong NgZone
        // để Angular biết mà update UI (nếu không UI sẽ bị đơ dù data đã về)
        this.zone.run(() => {
          try {
            // Chuyển cục text JSON thành mảng DTO
            const data: ReminderDto[] = JSON.parse(event.data);
            // Bắn data vào đường ống RxJS cho Store hứng
            observer.next(data);
          } catch (error) {
            console.error('Error when parse data from SSE Stream:', error);
          }
        });
      };

      // Nếu rớt mạng hoặc sập BE, EventSource sẽ tự động thử kết nối lại.
      // Mình chỉ log ra cho biết thôi.
      eventSource.onerror = (error) => {
        this.zone.run(() => {
          console.error('Connection error SSE, reconnecting...', error);
          // Không gọi observer.error(error) ở đây vì nếu gọi, luồng sẽ chết luôn và không tự reconnect được.
        });
      };

      // Dọn dẹp: Khúc này cực kỳ quan trọng!
      // Khi component/store bị hủy, RxJS sẽ gọi hàm này để khóa van nước lại, chống rỉ memory (Memory Leak).
      return () => {
        eventSource.close();
        console.log('SSE connection closed.');
      };
    });
  }
}
