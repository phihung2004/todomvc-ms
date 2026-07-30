using Carter;
using MongoDB.Entities;
using Todo.Api.Entities;

namespace Todo.Api.Features.Reminders.Dismiss
{
    public class DismissReminderEndpoint : ICarterModule
    {

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var remGroup = app.MapGroup("/api/reminders");


            remGroup.MapPatch("/{id}/dismiss", async (string id) => {
                var reminder = await DB.Find<Reminder>().OneAsync(id);

                if (reminder == null)
                {
                    return Results.NotFound();
                }

                reminder.State = ReminderState.Dismissed;

                //await reminder.SaveAsync();
                // Update như bên dưới thì sẽ xịn hơn

                // Gợi ý sửa cho DismissReminderEndpoint
                await DB.Update<Reminder>()
                    .MatchID(id)
                    .Modify(r => r.State, ReminderState.Dismissed)
                    .ExecuteAsync();

                return Results.NoContent();
            });

        }
    }
}
