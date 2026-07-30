using Carter;
using MongoDB.Entities;
using Todo.Api.Entities;

namespace Todo.Api.Features.Reminders.GetList
{
    public record GetRemindersResponse
    (
         string Id,           // vì bên Entity tự tạo ID nên bển không viết, nhưng bên này là POLO > viết
         string TodoId,         // ref TodoItem
         DateTime DueAt,
         ReminderState State,    // Pending | Snoozed | Dismissed
         DateTime? SnoozeUntil, 
         DateTime FiredAt       // lúc scanner phát hiện tới hạn
    );

    public class GetRemindersEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var remGroup = app.MapGroup("/api/reminders");

            remGroup.MapGet("", async (string? state) =>
            {
                List<Reminder> reminders = new List<Reminder>();

                if (state == "pending")
                {
                    reminders = await DB.Find<Reminder>()
                    .Match(r => r.State == ReminderState.Pending)
                    .ExecuteAsync();
                }

                var result = reminders.Select(r => new GetRemindersResponse(
                    r.ID,
                    r.TodoId,
                    r.DueAt,
                    r.State,
                    r.SnoozeUntil,
                    r.FiredAt // Nhớ là FiredAt chứ không phải FireAt nhé
                )).ToList();

                return Results.Ok(result);
            });

        }
    }
}
