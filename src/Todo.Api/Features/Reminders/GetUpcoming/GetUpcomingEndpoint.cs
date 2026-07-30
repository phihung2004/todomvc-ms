using Carter;
using MongoDB.Entities;
using Todo.Api.Common;
using Todo.Api.Entities;

namespace Todo.Api.Features.Reminders.GetUpcoming
{
    public class GetUpcomingEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var remGroup = app.MapGroup("/api/reminders");

            // Reminder không hề có tiêu đề
            // Nên cần mói luôn cả todo mà bằng id mà thằng reminder này đang gắng vào.
            remGroup.MapGet("/upcoming", async (string? within) =>
            {
                var now = DateTime.UtcNow;

                var next24h = now.AddHours(24);

                List<TodoItem> items = new List<TodoItem>();

                items = await DB.Find<TodoItem>()
                .Match(t => t.IsCompleted == false)
                .Match(t => t.DueAt > now)
                .Match(t => t.DueAt < next24h)
                .ExecuteAsync();

                var responses = items.Select(item => new TodoResponse(
                    item.ID,
                    item.Title,
                    item.IsCompleted,
                    item.CreateAt,
                    item.DueAt
                )).ToList();

                return Results.Ok(responses);

            });

        }
    }
}
