import { Component } from '@angular/core';
import { TodoInput } from '../features/todos/todo-input/todo-input';
import { Footer } from '../features/todos/footer/footer';
import { RouterOutlet } from '@angular/router';
import { NotificationBell } from '../features/reminders/notification-bell/notification-bell';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  imports: [TodoInput, Footer, RouterOutlet, NotificationBell],
})
export class App {}
