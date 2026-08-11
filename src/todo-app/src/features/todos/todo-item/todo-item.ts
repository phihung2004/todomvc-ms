import { Component, input, output } from '@angular/core';
import { TodoDto } from '../todo-api.service';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-todo-item',
  standalone: true,
  imports: [DatePipe],
  templateUrl: './todo-item.html',
  styleUrl: './todo-item.css', // SỬA CSS SMLLLLL. QUÊN BỎ VÔ, MÉO BIẾT
})

// THằng này khoogn nói chuyện với store, thông qua List
export class TodoItem {
  // Nó sẽ đòi 1 thằng Todo để hiện
  todo = input.required<TodoDto>();
  isEditing = false;

  // Mỗi 1 hành động thì 1 output khác nhau, gộp lại 1 thằng thì nó làm chung > Nổ
  itemToDelete = output<string>();
  itemToToggle = output<string>();
  itemToEdit = output<{ id: string; title: string; isCompleted: boolean; dueAt?: string }>();
  errorToClear = output<void>();

  // Khi nhấn nút Edit, nó sẽ bật chế độ edit
  startEdit(): void {
    this.isEditing = true;
    this.errorToClear.emit();
  }

  cancelEdit(): void {
    this.isEditing = false;
    this.errorToClear.emit();
  }

  submitEdit(newTitle: string, newDate: string): void {
    // Phải nhớ practice này, luôn trim input của người dùng đầu vào trước khi nấu
    const trimmedTitle = newTitle.trim();

    // Nếu trống thì xóa luôn
    if (!trimmedTitle) {
      this.itemToDelete.emit(this.todo().id);
      return;
    }

    // code gà: this.itemToDelete.emit(this.todo().id, newTitle, this.todo().isCompleted);
    this.itemToEdit.emit({
      id: this.todo().id,
      title: trimmedTitle,
      isCompleted: this.todo().isCompleted,
      dueAt: newDate ? newDate : undefined,
    });

    this.isEditing = false;
  }

  deleteItem(): void {
    this.itemToDelete.emit(this.todo().id);
  }

  // Event beenkia nên để là change thay vì click, đê rphuf hợp với các trình duyệt,
  // AI bảo thế :))
  toggleItem(): void {
    this.itemToToggle.emit(this.todo().id);
  }
}
