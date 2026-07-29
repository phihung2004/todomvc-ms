import { Component, inject, OnInit } from '@angular/core';
import { ReminderStore } from '../reminders/reminder.store';
import { AsyncPipe, DatePipe } from '@angular/common';

@Component({
  selector: 'app-upcoming-panel',
  imports: [AsyncPipe, DatePipe],
  templateUrl: './upcoming-panel.html',
  styleUrl: './upcoming-panel.css',
})
export class UpcomingPanel implements OnInit {
  // Lôi kho ra
  readonly store = inject(ReminderStore);

  isPanelOpen = false;

  ngOnInit() {
    // Gọi hàm loadUpcoming mà anh em mình định nghĩa trong Store ở bước trước
    // (Đảm bảo chú đã thêm hàm này vào reminder.store.ts rồi nhé)
    this.store.loadUpcoming();
  }

  togglePanel() {
    this.isPanelOpen = !this.isPanelOpen;
  }
}
