import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-stats-button',
  standalone: true,
  imports: [RouterLink], // Bắt buộc phải có để dùng được routerLink
  template: `
    <a routerLink="/stats" class="stat-button" title="Xem Thống Kê">
      <svg
        xmlns="http://www.w3.org/2000/svg"
        fill="none"
        viewBox="0 0 24 24"
        stroke-width="1.5"
        stroke="currentColor"
      >
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          d="M3 13.125C3 12.504 3.504 12 4.125 12h2.25c.621 0 1.125.504 1.125 1.125v6.75C7.5 20.496 6.996 21 6.375 21h-2.25A1.125 1.125 0 0 1 3 19.875v-6.75ZM9.75 8.625c0-.621.504-1.125 1.125-1.125h2.25c.621 0 1.125.504 1.125 1.125v11.25c0 .621-.504 1.125-1.125 1.125h-2.25a1.125 1.125 0 0 1-1.125-1.125V8.625ZM16.5 4.125c0-.621.504-1.125 1.125-1.125h2.25C20.496 3 21 3.504 21 4.125v15.75c0 .621-.504 1.125-1.125 1.125h-2.25a1.125 1.125 0 0 1-1.125-1.125V4.125Z"
        />
      </svg>
    </a>
  `,
  styles: [
    `
      .stat-button {
        background: transparent;
        border: none;
        cursor: pointer;
        color: #4d4d4d;
        padding: 5px;
        margin-right: 15px; /* Giãn cách với cái panel bên cạnh */
        transition: color 0.2s;
        display: inline-flex;
        align-items: center;
      }
      .stat-button:hover {
        color: #b83f45; /* Màu hover đỏ chuẩn TodoMVC */
      }
      svg {
        width: 24px;
        height: 24px;
      }
    `,
  ],
})
export class StatsButton {}
