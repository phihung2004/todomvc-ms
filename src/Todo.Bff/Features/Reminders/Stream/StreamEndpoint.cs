using Carter;

namespace Todo.Bff.Features.Reminders.Stream
{
    // SSE tuf 

    public class StreamEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var reminderGroup = app.MapGroup("/bff/reminders");

            reminderGroup.MapGet("/stream", async (HttpContext ctx, ReminderApiClient client, CancellationToken ct) =>
            {
                // 1. Cài đặt Header của ống nước SSE
                ctx.Response.Headers.Append("Content-Type", "text/event-stream");
                ctx.Response.Headers.Append("Cache-Control", "no-cache");
                ctx.Response.Headers.Append("Connection", "keep-alive");

                // 2. Bọc try-catch để hứng lúc ống nước bị gãy do FE F5 hoặc tắt Tab
                try
                {
                    while (!ct.IsCancellationRequested)
                    {
                        var response = await client.GetPendingReminderAsync("pending");

                        if (response.IsSuccessStatusCode)
                        {
                            var jsonRaw = await response.Content.ReadAsStringAsync(ct);
                            var sseData = $"data: {jsonRaw}\n\n";

                            await ctx.Response.WriteAsync(sseData, ct);
                            await ctx.Response.Body.FlushAsync(ct);
                        }

                        // Nếu FE tắt tab lúc BE đang ngủ 10 giây, hàm này sẽ nổ ra TaskCanceledException
                        await Task.Delay(TimeSpan.FromSeconds(10), ct);
                    }
                }
                // Hứng lỗi CancellationToken bị kích hoạt hủy
                catch (OperationCanceledException)
                {
                    // Chỗ này không làm gì sất, chỉ log ra cho dev biết là FE đã ngắt kết nối an toàn
                    Console.WriteLine("[BFF] Client dropped SSE connection peacefully. No worry!");
                }
                catch (Exception ex)
                {
                    // Hứng các lỗi khác (ví dụ sập DB, sập API)
                    Console.WriteLine($"[BFF] SSE Stream failed horribly: {ex.Message}");
                }
            });
        }
    }
}
