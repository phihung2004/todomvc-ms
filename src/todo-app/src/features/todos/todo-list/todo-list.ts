import { Component, OnInit, inject } from '@angular/core';
import { TodoItem } from '../todo-item/todo-item';
import { TodosStore } from '../todos.store';
import { AsyncPipe } from '@angular/common';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-todo-list',
  imports: [TodoItem, AsyncPipe],
  templateUrl: './todo-list.html',
})
export class TodoList implements OnInit {
  store = inject(TodosStore);
  route = inject(ActivatedRoute);

  // Todo: Lấy được active count và completed count từ store
  // NHƯNG khi toggle/toggle all lên store thì active count không update lại
  // Cần phải subscribe vào store để lấy được active count và completed count mới nhất
  ngOnInit(): void {
    //this.store.loadTodos();

    this.route.paramMap.subscribe((param) => {
      const currentFIlter = param.get('filter') as 'all' | 'active' | 'completed';
      // if (currentFIlter) {
      //   this.store.setFilter(currentFIlter);
      // }

      this.store.setFilter(currentFIlter || 'all');

      this.store.loadTodos();
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
