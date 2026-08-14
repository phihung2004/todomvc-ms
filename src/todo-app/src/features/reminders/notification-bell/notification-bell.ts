import { Component, inject, OnInit } from '@angular/core';
import { ReminderStore } from '../reminder.store';
import { AsyncPipe } from '@angular/common';
import { BellItem } from '../bell-item/bell-item';
import { TodosStore } from '../../todos/todos.store';
import { combineLatest, map, Observable } from 'rxjs';

@Component({
  selector: 'app-notification-bell',
  imports: [AsyncPipe, BellItem],
  templateUrl: './notification-bell.html',
  styleUrl: './notification-bell.css',
})
export class NotificationBell implements OnInit {
  readonly store = inject(ReminderStore);
  private readonly todoStore = inject(TodosStore);
  // 2 thằng trên dang readonly. Không làm phụ thuộc gì gì đó quên mẹ r

  isPanelOpen = false;

  readonly remindersWithTitle$ = combineLatest([this.store.pending$, this.todoStore.todos$]).pipe(
    map(([pending, todos]) => {
      // Dùng Map để lookup O(1) thay vì .find() O(n) cho từng reminder
      const titleMap = new Map(todos.map((t) => [t.id, t.title]));
      return pending.map((r) => ({
        ...r,
        title: titleMap.get(r.todoId) ?? 'Đang tải (hoặc đã xóa)...',
      }));
    }),
  );

  ngOnInit() {
    this.store.connectStream();
  }

  togglePanel() {
    this.isPanelOpen = !this.isPanelOpen;
  }

  // Hứng event từ Dumb component rồi phi thẳng lên Store
  snooze(payload: { id: string; minutes: number }) {
    this.store.snoozeTodo(payload);
  }

  dismiss(id: string) {
    this.store.dismissReminder(id);
  }

  // Chuyển hàm lấy Title từ panel cũ sang đây
  // getTodoTitle(todoId: string): Observable<string> {
  //   return this.todoStore.todos$.pipe(
  //     map((todos) => {
  //       const found = todos.find((t) => t.id === todoId);
  //       return found ? found.title : 'Đang tải (hoặc đã xóa)...';
  //     }),
  //   );
  // }
}
