import { Component, inject, OnInit } from '@angular/core';
import { ReminderStore } from '../reminder.store';
import { AsyncPipe, DatePipe } from '@angular/common';

@Component({
  selector: 'app-reminder-panel',
  imports: [AsyncPipe, DatePipe],
  templateUrl: './reminder-panel.html',
  styleUrl: './reminder-panel.css',
})
export class ReminderPanel implements OnInit {
  readonly store = inject(ReminderStore);

  // Khởi tạo trạng thái ĐÓNG
  isPanelOpen = false;

  ngOnInit() {
    this.store.loadUpcoming();
  }

  // Hàm bật/tắt bảng
  togglePanel() {
    this.isPanelOpen = !this.isPanelOpen;
  }
}
