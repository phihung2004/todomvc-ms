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

  ngOnInit(): void {
    this.store.loadTodos();

    this.route.paramMap.subscribe((param) => {
      const currentFIlter = param.get('filter') as 'all' | 'active' | 'completed';
      if (currentFIlter) {
        this.store.setFilter(currentFIlter);
      }
    });
  }

  deleteTodo(id: string): void {
    this.store.deleteTodo(id);
  }

  toggleItem(id: string): void {
    this.store.toggleTodo(id);
  }

  editTodo({ id, title, isCompleted }: { id: string; title: string; isCompleted: boolean }): void {
    this.store.updateTodo({ id, title, isCompleted });
  }

  toggleAll(event: Event): void {
    const checkbox = event.target as HTMLInputElement;
    this.store.toggleAllTodos(checkbox.checked);
  }
}
