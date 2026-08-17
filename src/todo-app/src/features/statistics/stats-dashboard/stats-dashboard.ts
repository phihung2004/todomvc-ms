import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { StatsStore } from '../stats.store';

@Component({
  selector: 'app-stats-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  providers: [StatsStore], // Fix lỗi L8, scope store gắn liền với component
  templateUrl: './stats-dashboard.html',
  styleUrls: ['./stats-dashboard.css'],
})
export class StatsDashboard implements OnInit {
  readonly store = inject(StatsStore);

  ngOnInit(): void {
    // Kích hoạt effect lấy data khi mở trang
    this.store.loadOverview();
  }

  // Hàm phụ trợ tính giá trị cột cao nhất để scale biểu đồ CSS
  getMaxCount(completedByDay: any[]): number {
    if (!completedByDay || completedByDay.length === 0) return 1;
    const max = Math.max(...completedByDay.map((d) => d.count));
    return max === 0 ? 1 : max;
  }
}
