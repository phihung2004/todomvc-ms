using Carter;
using MediatR;

namespace Todo.Api.Features.Reminders.Snooze;

public class SnoozeReminderEndpoint : ICarterModule
{
    // Wow, cái này gọn vl
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/reminders/{id}/snooze", async (string id, SnoozeReminderRequest request, IMediator mediator) =>
        {
            var command = new SnoozeReminderCommand(id, request.Minutes);

            var success = await mediator.Send(command);

            if (!success)
            {
                return Results.NotFound();
            }

            return Results.NoContent();
        });
    }
}