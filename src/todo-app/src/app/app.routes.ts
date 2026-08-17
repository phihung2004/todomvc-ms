import { Routes } from '@angular/router';
import { TodoList } from '../features/todos/todo-list/todo-list';
// Tạm thời comment dòng dưới lại cho đến khi anh em mình code xong cái UI
import { StatsDashboard } from '../features/statistics/stats-dashboard/stats-dashboard';

export const routes: Routes = [
  // [QUAN TRỌNG]: Đặt /stats lên đầu để không bị /:filter nuốt chửng
  { path: 'stats', component: StatsDashboard },
  { path: ':filter', component: TodoList },
  { path: '', redirectTo: '/all', pathMatch: 'full' },
];
