using MediatR;
using MongoDB.Driver;
using MongoDB.Entities;
using Todo.Api.Entities;

namespace Todo.Api.Features.Statistics
{
    public class StatsOverviewHandler : IRequestHandler<StatsOverviewQuery, StatsOverviewResponse>
    {
        public async Task<StatsOverviewResponse> Handle(StatsOverviewQuery request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var today = now.Date; // Ra số ngày (vd: Ngày 4 thì ra kiểu DateTime: 2026-08-04 00:00:00). Và nó sẽ đổi luôn phần giờ thành > 00:00:00

            // Vãi, Chủ nhật(0) > Thứ Hai(1) > Thứ 3(2) > ... > Thứ Bảy(6)
            // lấy Thứ 3(2) - Thứ 2(1) = 1 => Thứ 3 cách Thứ 2 1 ngày > ĐÚNG!
            // Nhưng còn Chủ Nhật thì đang bị bủ:
            // Chủ nhât (0) - Thứ 2(1) = -1 => Cái này là tiến về tương lai, lấy thứ 2 của tuần sau rồi.
            // Nên công thức là ((7 + X) % 7) => chia lấy dư, lấy phần thừa auto ra đúng phần cần
            // Ép kiểu về int để nó trừ nhau
            int daysToMonday = ((7 + ((int)today.DayOfWeek - (int)DayOfWeek.Monday)) % 7);
            var startOfWeek = today.AddDays(-1 * daysToMonday); // nghịch âm lại để "lùi" ngày từ toán Add.

            var last7Days = today.AddDays(-6);

            // ==========================================
            // 1. CHẠY SONG SONG CÁC CÂU ĐẾM NHANH (Concurrent Queries)
            // ==========================================
            var totalTask = DB.CountAsync<TodoItem>(_ => true, cancellation: cancellationToken);
            var activeTask = DB.CountAsync<TodoItem>(t => !t.IsCompleted, cancellation: cancellationToken);
            var completedTask = DB.CountAsync<TodoItem>(t => t.IsCompleted, cancellation: cancellationToken);
            var overdueTask = DB.CountAsync<TodoItem>(t => !t.IsCompleted && t.DueAt < now, cancellation: cancellationToken);
            var completedTodayTask = DB.CountAsync<TodoItem>(t => t.IsCompleted && t.CompletedAt >= today, cancellation: cancellationToken);
            var completedThisWeekTask = DB.CountAsync<TodoItem>(t => t.IsCompleted && t.CompletedAt >= startOfWeek, cancellation: cancellationToken);

            // ==========================================
            // 2. AGGREGATION PIPELINE: Gom nhóm đếm số lượng theo từng ngày
            // ==========================================
            // Lệnh DB.Fluent() giúp móc thẳng vào Aggregation Pipeline của MongoDB
            var dailyChartTask = DB.Fluent<TodoItem>()
                .Match(t => t.IsCompleted && t.CompletedAt >= last7Days) // Lọc: Chỉ lấy đồ đã xong trong 7 ngày qua, xong đem mớ đó đi xuống Group
                .Group(
                    // Gom nhóm theo Năm - Tháng - Ngày
                    // Trả về theo kiểu: { Year: 2026, Month: 8, Day: 13 }
                    t => new
                    {
                        Year = t.CompletedAt!.Value.Year,
                        Month = t.CompletedAt.Value.Month,
                        Day = t.CompletedAt.Value.Day
                    },
                    // Đếm số lượng
                    g => new
                    {
                        DateKey = g.Key,
                        Count = g.Count()
                    }
                )
                .ToListAsync(cancellationToken);

            // Chờ TẤT CẢ các luồng chạy xong cùng một lúc
            await Task.WhenAll(
                totalTask, activeTask, completedTask,
                overdueTask, completedTodayTask, completedThisWeekTask, dailyChartTask
            );


            // ==========================================
            // 3. TỔNG HỢP & LÀM MƯỢT DATA CHO FRONTEND
            // ==========================================
            // ÉP KIỂU (int) Ở ĐÂY VÌ MONGODB TRẢ VỀ LONG
            var total = (int)totalTask.Result;
            var completed = (int)completedTask.Result;

            // Tính Rate (Tránh chia cho 0)
            var completionRate = total == 0 ? 0 : Math.Round((double)completed / total * 100, 2);

            // Map data biểu đồ: Nếu ngày nào không có task hoàn thành, DB sẽ không trả về, ta phải tự điền số 0 vào
            var rawChartData = dailyChartTask.Result;
            var chartData = new List<DailyCount>();

            for (int i = 0; i < 7; i++)
            {
                var currentDate = last7Days.AddDays(i);

                // Tìm trong mảng Aggregation xem ngày này có data không
                var match = rawChartData.FirstOrDefault(x =>
                    x.DateKey.Year == currentDate.Year &&
                    x.DateKey.Month == currentDate.Month &&
                    x.DateKey.Day == currentDate.Day);

                chartData.Add(new DailyCount(
                    DateOnly.FromDateTime(currentDate),
                    (int)(match?.Count ?? 0) // Ép kiểu luôn chỗ này cho chắc cú
                ));
            }

            // Why parse về int
            // Vì MongoDB nó mặc định là Long. MÌnh thì chỉ cần int là ngon r
            return new StatsOverviewResponse(
                Total: total, // Đã ép kiểu ở trên
                Active: (int)activeTask.Result, // Ép kiểu trực tiếp
                Completed: completed, // Đã ép kiểu ở trên
                Overdue: (int)overdueTask.Result,
                CompletedToday: (int)completedTodayTask.Result,
                CompletedThisWeek: (int)completedThisWeekTask.Result,
                CompletionRate: completionRate,
                CompletedByDay: chartData
            );
        }
    }
}
