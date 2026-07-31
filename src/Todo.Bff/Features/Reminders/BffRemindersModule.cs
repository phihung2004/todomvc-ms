//using Carter;
//using Todo.Bff.Extensions;

//namespace Todo.Bff.Features.Reminders
//{
//    // Thằng này hứng các request từ FE
//    public class BffRemindersModule : ICarterModule
//    {
//        public void AddRoutes(IEndpointRouteBuilder app)
//        {
//            var reminderGroup = app.MapGroup("/bff/reminders");

//            //reminderGroup.MapGet("", async (string? state, ReminderApiClient client) =>
//            //{
//            //    var response = await client.GetPendingReminderAsync(state);
//            //    return await response.ToResultAsync();
//            //});

//            //reminderGroup.MapGet("/upcoming", async (string? within, ReminderApiClient client) =>
//            //{
//            //    var response = await client.GetUpcomingReminderAsync(within);

//            //    return await response.ToResultAsync();
//            //});

//            //reminderGroup.MapPatch("/{id}/snooze", async (string id, SnoozeReminderRequest request, ReminderApiClient client) =>
//            //{
//            //    var response = await client.SnoozeReminderAsync(id, request);

//            //    return await response.ToResultAsync();
//            //});

//            //reminderGroup.MapPatch("/{id}/dismiss", async (string id, ReminderApiClient client) =>
//            //{
//            //    var response = await client.DismissReminderAsync(id);

//            //    return await response.ToResultAsync();
//            //});


//            //reminderGroup.MapGet("/stream", async (HttpContext ctx, ReminderApiClient client, CancellationToken ct) =>
//            //{
//            //    // 1. Cài đặt Header của ống nước SSE
//            //    ctx.Response.Headers.Append("Content-Type", "text/event-stream");
//            //    ctx.Response.Headers.Append("Cache-Control", "no-cache");
//            //    ctx.Response.Headers.Append("Connection", "keep-alive");

//            //    // 2. Bọc try-catch để hứng lúc ống nước bị gãy do FE F5 hoặc tắt Tab
//            //    try
//            //    {
//            //        while (!ct.IsCancellationRequested)
//            //        {
//            //            var response = await client.GetPendingReminderAsync("pending");

//            //            if (response.IsSuccessStatusCode)
//            //            {
//            //                var jsonRaw = await response.Content.ReadAsStringAsync(ct);
//            //                var sseData = $"data: {jsonRaw}\n\n";

//            //                await ctx.Response.WriteAsync(sseData, ct);
//            //                await ctx.Response.Body.FlushAsync(ct);
//            //            }

//            //            // Nếu FE tắt tab lúc BE đang ngủ 10 giây, hàm này sẽ nổ ra TaskCanceledException
//            //            await Task.Delay(TimeSpan.FromSeconds(10), ct);
//            //        }
//            //    }
//            //    // Hứng lỗi CancellationToken bị kích hoạt hủy
//            //    catch (OperationCanceledException)
//            //    {
//            //        // Chỗ này không làm gì sất, chỉ log ra cho dev biết là FE đã ngắt kết nối an toàn
//            //        Console.WriteLine("[BFF] Client dropped SSE connection peacefully. No worry!");
//            //    }
//            //    catch (Exception ex)
//            //    {
//            //        // Hứng các lỗi khác (ví dụ sập DB, sập API)
//            //        Console.WriteLine($"[BFF] SSE Stream failed horribly: {ex.Message}");
//            //    }
//            //});
//        }
//    }
//}
