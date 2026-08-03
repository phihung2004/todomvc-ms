import { Component, inject, OnInit } from '@angular/core';
import { ReminderStore } from '../reminder.store';
import { AsyncPipe } from '@angular/common';
import { BellItem } from '../bell-item/bell-item';
import { TodosStore } from '../../todos/todos.store';
import { map, Observable } from 'rxjs';

@Component({
  selector: 'app-notification-bell',
  imports: [AsyncPipe, BellItem],
  templateUrl: './notification-bell.html',
  styleUrl: './notification-bell.css',
})
export class NotificationBell implements OnInit {
  readonly store = inject(ReminderStore);
  private readonly todoStore = inject(TodosStore);

  isPanelOpen = false;

  ngOnInit() {
    this.store.connectStream();
  }

  togglePanel() {
    this.isPanelOpen = !this.isPanelOpen;
  }

  // Chuyển hàm lấy Title từ panel cũ sang đây
  getTodoTitle(todoId: string): Observable<string> {
    return this.todoStore.todos$.pipe(
      map((todos) => {
        const found = todos.find((t) => t.id === todoId);
        return found ? found.title : 'Đang tải (hoặc đã xóa)...';
      }),
    );
  }

  // Hứng event từ Dumb component rồi phi thẳng lên Store
  snooze(payload: { id: string; minutes: number }) {
    this.store.snoozeTodo(payload);
  }

  dismiss(id: string) {
    this.store.dismissReminder(id);
  }
}
