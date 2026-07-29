import { Component, inject, OnInit } from '@angular/core';
import { ReminderStore } from '../reminder.store';
import { AsyncPipe } from '@angular/common';
import { ReminderPanel } from '../reminder-panel/reminder-panel';

@Component({
  selector: 'app-notification-bell',
  imports: [AsyncPipe, ReminderPanel],
  templateUrl: './notification-bell.html',
  styleUrl: './notification-bell.css',
})
export class NotificationBell implements OnInit {
  // Lôi cái kho thông báo ra xài
  readonly store = inject(ReminderStore);

  // Biến này để nhớ xem Panel đang mở hay đóng
  isPanelOpen = false;

  ngOnInit() {
    // Vừa bật app lên là cắm ống nước lấy thông báo Real-time luôn!
    this.store.connectStream();
  }

  togglePanel() {
    this.isPanelOpen = !this.isPanelOpen;
  }
}
