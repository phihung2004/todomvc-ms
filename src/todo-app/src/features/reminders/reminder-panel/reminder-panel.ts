import { Component, inject } from '@angular/core';
import { ReminderStore } from '../reminder.store';
import { TodosStore } from '../../todos/todos.store';
import { map, Observable } from 'rxjs';
import { AsyncPipe, DatePipe } from '@angular/common';

@Component({
  selector: 'app-reminder-panel',
  imports: [AsyncPipe, DatePipe],
  templateUrl: './reminder-panel.html',
  styleUrl: './reminder-panel.css',
})
export class ReminderPanel {
  readonly reminderStore = inject(ReminderStore);

  // Inject thêm anh bạn hàng xóm vào để mượn data
  private readonly todoStore = inject(TodosStore);

  // Trick "Hỏi đường": Đưa ID vào, lục trong mảng todos tìm ra Title
  getTodoTitle(todoId: string): Observable<string> {
    return this.todoStore.todos$.pipe(
      map((todos) => {
        const found = todos.find((t) => t.id === todoId);
        return found ? found.title : 'Đang tải (hoặc đã xóa)...';
      }),
    );
  }

  // Ép kiểu string từ HTML về Number rồi quăng cho Store
  snooze(id: string, minutes: string) {
    this.reminderStore.snoozeTodo({ id, minutes: Number(minutes) });
  }

  dismiss(id: string) {
    this.reminderStore.dismissReminder(id);
  }
}
