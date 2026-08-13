import { RenderMode, ServerRoute } from '@angular/ssr';

export const serverRoutes: ServerRoute[] = [
  {
    path: '**',
    // renderMode: RenderMode.Prerender

    // [FIX L6]: Đổi sang Server (SSR) để xử lý các route động có tham số như :filter,
    // tránh lỗi crash khi chạy lệnh `ng build`
    renderMode: RenderMode.Server,
  },
];
