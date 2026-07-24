import { Component, input, output } from '@angular/core';
import { TodoDto } from '../../../core/todo-api.service';

@Component({
  selector: 'app-todo-item',
  standalone: true,
  imports: [],
  templateUrl: './todo-item.html',
})

// THằng này khoogn nói chuyện với store, thông qua List
export class TodoItem {
  // Nó sẽ đòi 1 thằng Todo để hiện
  todo = input.required<TodoDto>();
  isEditing = false;


  // Mỗi 1 hành động thì 1 output khác nhau, gộp lại 1 thằng thì nó làm chung > Nổ
  itemToDelete = output<string>();
  itemToToggle = output<string>();
  itemToEdit = output<{id:string,title:string, isCompleted:boolean}>();

  // Khi nhấn nút Edit, nó sẽ bật chế độ edit
  startEdit(): void{
      this.isEditing = true;
  }

  cancelEdit(): void{
    this.isEditing = false;
  }


  submitEdit(newTitle:string): void{
    // Phải nhớ practice này, luôn trim input của người dùng đầu vào trước khi nấu
    const trimmedTitle = newTitle.trim();

    // Nếu trống thì xóa luôn
    if (!trimmedTitle) {
      this.itemToDelete.emit(this.todo().id);
    }

    // code gà: this.itemToDelete.emit(this.todo().id, newTitle, this.todo().isCompleted);
    this.itemToEdit.emit({
      id:this.todo().id, 
      title:trimmedTitle, 
      isCompleted:this.todo().isCompleted
    });

    this.isEditing = false;
  }

  deleteItem(): void{
      this.itemToDelete.emit(this.todo().id);
  }

  // Event beenkia nên để là change thay vì click, đê rphuf hợp với các trình duyệt,
  // AI bảo thế :))
  toggleItem(): void{
      this.itemToToggle.emit(this.todo().id);
  }

}
