import { Component, inject, OnInit } from '@angular/core';
import { TodosStore } from '../todos.store';
import { AsyncPipe } from '@angular/common';

@Component({
  selector: 'app-todo-input',
  standalone: true,
  imports: [AsyncPipe],
  templateUrl: './todo-input.html',
  styleUrl: './todo-input.css',
})
export class TodoInput {
  store = inject(TodosStore);

  toggleAll(event: Event): void {
    const checkbox = event.target as HTMLInputElement;
    this.store.toggleAllTodos(checkbox.checked);
  }

  createItem(event: Event): void {
    const input = event.target as HTMLInputElement;

    this.store.createTodo(input.value.trim());

    // Đừng tự xóa trắng ô input vội.
    // Nếu có lỗi, ta vẫn muốn giữ lại chữ cũ để user sửa.
    // Việc xóa trắng ô input khi tạo thành công có thể xử lý thông qua việc lắng nghe Store,
    // hoặc đơn giản là set timeout nhẹ, nhưng chuẩn nhất là giữ nguyên nếu đang báo lỗi.
    if (input.value.trim().length > 0 && input.value.trim().length <= 200) {
      input.value = '';
    }
  }

  clearError(): void {
    // Khi user bắt đầu gõ ký tự mới, gọi Store để dọn sạch câu báo lỗi màu đỏ
    this.store.setError(null);
  }
}
