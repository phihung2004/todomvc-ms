import { Component, inject, OnInit } from '@angular/core';
import { TodosStore } from '../todos.store';
import { AsyncPipe } from '@angular/common';
import { NotificationBell } from '../../reminders/notification-bell/notification-bell';

@Component({
  selector: 'app-todo-input',
  standalone: true,
  imports: [AsyncPipe],
  templateUrl: './todo-input.html',
  styleUrl: './todo-input.css',
})
export class TodoInput {
  store = inject(TodosStore);

  // Toggle ALL nên để bên list thay vì input, tại sao ? Nút trên Demo nằm ngang input mà?
  //
  // toggleAll(event: Event): void {
  //   const checkbox = event.target as HTMLInputElement;
  //   this.store.toggleAllTodos(checkbox.checked);
  // }

  createItem(titleInput: HTMLInputElement, dateInput: HTMLInputElement): void {
    const title = titleInput.value.trim();
    const dueAt = dateInput.value;

    if (!title) {
      this.store.setError('Todo can not leave empty!');
      return;
    }

    // ÉP KIỂU MÚI GIỜ: Chuyển giờ Local thành chuỗi chuẩn UTC (ISO 8601)
    const parsedDueAt = dueAt ? new Date(dueAt).toISOString() : undefined;

    this.store.createTodo({
      title,
      dueAt: parsedDueAt,
    });

    // Đừng tự xóa trắng ô input vội.
    // Nếu có lỗi, ta vẫn muốn giữ lại chữ cũ để user sửa.
    // Việc xóa trắng ô input khi tạo thành công có thể xử lý thông qua việc lắng nghe Store,
    // hoặc đơn giản là set timeout nhẹ, nhưng chuẩn nhất là giữ nguyên nếu đang báo lỗi.
    if (title.length > 0 && title.length <= 200) {
      titleInput.value = '';
      dateInput.value = '';
    }
  }

  clearError(): void {
    // Khi user bắt đầu gõ ký tự mới, gọi Store để dọn sạch câu báo lỗi màu đỏ
    this.store.setError(null);
  }
}
