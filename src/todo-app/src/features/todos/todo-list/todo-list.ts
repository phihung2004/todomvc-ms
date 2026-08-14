import { Component, OnInit, inject } from '@angular/core';
import { TodoItem } from '../todo-item/todo-item';
import { TodosStore } from '../todos.store';
import { AsyncPipe } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { Footer } from '../footer/footer';
import { TodoInput } from '../todo-input/todo-input';

@Component({
  selector: 'app-todo-list',
  imports: [TodoItem, AsyncPipe, Footer, TodoInput],
  templateUrl: './todo-list.html',
})
export class TodoList implements OnInit {
  store = inject(TodosStore);
  route = inject(ActivatedRoute);

  ngOnInit(): void {
    // Kéo toàn bộ data 1 lần duy nhất vào State
    this.store.loadTodos();

    // Nghe URL đổi: Chỉ cập nhật trạng thái Filter trong store.
    // Selector `filteredTodos$` sẽ tự động chạy và lọc list trên UI (0ms latency, không gọi BE).
    this.route.paramMap.subscribe((param) => {
      const currentFilter = param.get('filter') as 'all' | 'active' | 'completed';
      this.store.setFilter(currentFilter || 'all');
    });
  }

  deleteTodo(id: string): void {
    this.store.deleteTodo(id);
  }

  toggleItem(id: string): void {
    this.store.toggleTodo(id);
  }

  editTodo({
    id,
    title,
    isCompleted,
    dueAt,
  }: {
    id: string;
    title: string;
    isCompleted: boolean;
    dueAt?: string;
  }): void {
    this.store.updateTodo({ id, title, isCompleted, dueAt });
  }

  toggleAll(event: Event): void {
    const checkbox = event.target as HTMLInputElement;
    this.store.toggleAllTodos(checkbox.checked);
  }
}
