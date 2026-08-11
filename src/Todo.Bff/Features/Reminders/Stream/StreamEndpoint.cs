using Carter;

namespace Todo.Bff.Features.Reminders.Stream
{
    // SSE tuf 
    // FE có request 1 lần đầu, sau đó thfi cả 2 sẽ kết nối luôn với nhau.
    // Vậy thì kết nối với BFF thôi ha, vì SSE chủ dừng lại ở nó, việc còn lại của BFF là gọi về Api, nên coi như là FE chỉ nói với BFF đúng như nhiệm vụ của 1 Proxy BFF.
    public class StreamEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var reminderGroup = app.MapGroup("/bff/reminders");

            // ctx: nắm toàn bộ thông tin về 1 dòng đời của Request và Respone. Mình dùng để thêm vào các header để có thể làm SSE
            // client: code dùng httpclient để giao tiếp với BE, vì BFF không có DB
            // ct: token để
            reminderGroup.MapGet("/stream", async (HttpContext ctx, ReminderApiClient client, CancellationToken ct) =>
            {
                // Tự setup 1 HttpContext Header để gửi lên FE.
                // Dùng để thay đổi định dạng và quy tắc của gói tin trả về.
                // Bìa thư ?
                ctx.Response.Headers.Append("Content-Type", "text/event-stream"); // đây là 1 stream, để không đóng
                ctx.Response.Headers.Append("Cache-Control", "no-cache"); // Khỏi cache (cache là dùng để mà gom tập tin đủ lớn rồi mới gửi á hả ? Giống con tụ điện)
                ctx.Response.Headers.Append("Connection", "keep-alive"); // Báo Kestrel và Hệ điều hành là vẫn còn dùng kết nối này, đùng kill nó.

                // 2. Bọc try-catch để hứng lúc ống nước bị gãy do FE F5 hoặc tắt Tab
                try
                {
                    // Mở 1 lần duy nhất — không còn vòng lặp tự gọi lại như bản polling cũ
                    await using var apiStream = await client.GetReminderStreamAsync(ct);
                    using var reader = new StreamReader(apiStream);

                    // Đọc từng dòng Api chảy xuống, relay y nguyên xuống FE
                    while (!reader.EndOfStream && !ct.IsCancellationRequested)
                    {
                        var line = await reader.ReadLineAsync(ct);

                        if (line is not null)
                        {
                            await ctx.Response.WriteAsync(line + "\n", ct);
                            await ctx.Response.Body.FlushAsync(ct);
                        }
                    }
                }
                // Hứng lỗi CancellationToken bị kích hoạt hủy
                catch (OperationCanceledException)
                {
                    // Chỗ này không làm gì, chỉ log ra cho dev biết là FE đã ngắt kết nối an toàn
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



// Cục Polling , không dùng nữa 
//while (!ct.IsCancellationRequested)
//{
//    var response = await client.GetPendingReminderAsync("pending");

//    if (response.IsSuccessStatusCode)
//    {
//        // Lấy được mớ Pending Reminder thì gói thành JSon
//        var jsonRaw = await response.Content.ReadAsStringAsync(ct);

//        // Trường dữ liệu phải như format bên dưới
//        // Gói mớ JSOn lại đúng với format bên dưới.
//        var sseData = $"data: {jsonRaw}\n\n";

//        await ctx.Response.WriteAsync(sseData, ct);
//        await ctx.Response.Body.FlushAsync(ct);
//    }

//    // Nếu FE tắt tab lúc BE đang ngủ 10 giây, hàm này sẽ nổ ra TaskCanceledException
//    await Task.Delay(TimeSpan.FromSeconds(10), ct);
//}