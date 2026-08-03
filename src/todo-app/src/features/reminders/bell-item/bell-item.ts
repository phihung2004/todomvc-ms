import { Component, input, output } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ReminderDto } from '../reminder-api.service';

@Component({
  selector: 'app-bell-item',
  standalone: true,
  imports: [DatePipe],
  templateUrl: './bell-item.html',
  styleUrl: './bell-item.css', // Bắt buộc phải có dòng này CSS mới ăn!
})
export class BellItem {
  // Dùng signal input.required giống hệt todo-item
  item = input.required<ReminderDto>();
  title = input.required<string | null>();

  // Dùng signal output
  snooze = output<{ id: string; minutes: number }>();
  dismiss = output<string>();

  onSnooze(minutes: number) {
    // Gọi value từ signal bằng cặp ngoặc ()
    this.snooze.emit({ id: this.item().id, minutes });
  }

  onDismiss() {
    this.dismiss.emit(this.item().id);
  }
}
