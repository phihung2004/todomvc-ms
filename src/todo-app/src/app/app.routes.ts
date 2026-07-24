import { Routes } from '@angular/router';
import { TodoList } from '../features/todos/todo-list/todo-list';

export const routes: Routes = [
    { path: ':filter', component: TodoList},
    { path: '', redirectTo: '/all', pathMatch: 'full' }
];
