import { inject, Injectable } from '@angular/core';
import { ComponentStore } from '@ngrx/component-store';
import { Observable, timer, EMPTY } from 'rxjs';
import { catchError, switchMap, tap } from 'rxjs/operators';
import { StatsApiService, StatsOverviewDto } from '../statistics/stats-api.service';

// Định nghĩa State
export interface StatsState {
  overview: StatsOverviewDto | null;
  loading: boolean;
}

@Injectable()
export class StatsStore extends ComponentStore<StatsState> {
  private readonly statsApi = inject(StatsApiService);

  constructor() {
    // Khởi tạo state mặc định
    super({ overview: null, loading: false });
  }

  // =====================
  // SELECTORS (Dùng Signal cho Angular 16+)
  // =====================
  readonly overview = this.selectSignal((state) => state.overview);
  readonly loading = this.selectSignal((state) => state.loading);

  // =====================
  // UPDATERS
  // =====================
  readonly setOverview = this.updater((state, overview: StatsOverviewDto) => ({
    ...state,
    overview,
    loading: false,
  }));

  readonly setLoading = this.updater((state, loading: boolean) => ({
    ...state,
    loading,
  }));

  // =====================
  // EFFECTS
  // =====================
  // Khởi chạy load data và Tự động làm mới (Auto-refresh) mỗi 30 giây
  readonly loadOverview = this.effect<void>((trigger$) =>
    trigger$.pipe(
      // Chạy ngay lập tức (0ms) và lặp lại sau mỗi 30 giây (30000ms)
      switchMap(() => timer(0, 30000)),
      tap(() => this.setLoading(true)),
      switchMap(() =>
        this.statsApi.getOverview().pipe(
          tap((data) => this.setOverview(data)),
          catchError((error) => {
            console.error('Lỗi khi tải dữ liệu thống kê:', error);
            this.setLoading(false);
            return EMPTY;
          }),
        ),
      ),
    ),
  );
}
