import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../core/environments/environment';

// Khớp 100% với file JSON API nãy anh em mình test
export interface DailyCountDto {
  date: string;
  count: number;
}

export interface StatsOverviewDto {
  total: number;
  active: number;
  completed: number;
  overdue: number;
  completedToday: number;
  completedThisWeek: number;
  completionRate: number;
  completedByDay: DailyCountDto[];
}

@Injectable({
  providedIn: 'root',
})
export class StatsApiService {
  // Trỏ đúng vào nhánh /stats của BFF
  private readonly apiUrl = environment.apiUrl + '/stats';

  private http = inject(HttpClient);

  getOverview(): Observable<StatsOverviewDto> {
    return this.http.get<StatsOverviewDto>(`${this.apiUrl}/overview`);
  }
}
