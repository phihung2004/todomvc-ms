using Carter;
using MediatR;
using Todo.Api.Features.Reminders.GetList;
using Todo.Api.Features.Reminders.Messaging;

namespace Todo.Api.Features.Reminders.Stream
{
    public class StreamEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/reminders/stream", async (
                HttpContext ctx,
                IMediator mediator,
                ReminderStreamChannel streamChannel,
                CancellationToken ct) =>
            {
                ctx.Response.Headers.Append("Content-Type", "text/event-stream");
                ctx.Response.Headers.Append("Cache-Control", "no-cache");
                ctx.Response.Headers.Append("Connection", "keep-alive");

                try
                {
                    // Đẩy 1 lần ngay lúc mới connect, để client có data ngay, không phải chờ tín hiệu đầu tiên
                    await PushPendingReminders(mediator, ctx, ct);

                    // ReadAllAsync tự "await" cho tới khi có tín hiệu mới — KHÔNG polling, không Task.Delay
                    await foreach (var _ in streamChannel.Reader.ReadAllAsync(ct))
                    {
                        await PushPendingReminders(mediator, ctx, ct);
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("[Api] Client dropped SSE connection peacefully.");
                }
            });
        }

        private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        };

        private static async Task PushPendingReminders(IMediator mediator, HttpContext ctx, CancellationToken ct)
        {
            var reminders = await mediator.Send(new GetRemindersQuery("pending"), ct);

            var json = System.Text.Json.JsonSerializer.Serialize(reminders, JsonOptions);
            await ctx.Response.WriteAsync($"data: {json}\n\n", ct);
            await ctx.Response.Body.FlushAsync(ct);
        }
    }
}