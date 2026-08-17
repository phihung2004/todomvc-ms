import { Component } from '@angular/core';
import { TodoInput } from '../features/todos/todo-input/todo-input';
import { Footer } from '../features/todos/footer/footer';
import { RouterOutlet } from '@angular/router';
import { NotificationBell } from '../features/reminders/notification-bell/notification-bell';
import { ReminderPanel } from '../features/reminders/reminder-panel/reminder-panel';
import { StatsButton } from '../features/statistics/stats-button/stats-button';
import { TodosStore } from '../features/todos/todos.store';
import { ReminderStore } from '../features/reminders/reminder.store';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  imports: [RouterOutlet, NotificationBell, ReminderPanel, StatsButton],
  providers: [TodosStore, ReminderStore],
})
export class App {}
