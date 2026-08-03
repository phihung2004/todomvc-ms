using Carter;
using MediatR;

namespace Todo.Api.Features.Reminders.Dismiss;

public class DismissReminderEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/reminders/{id}/dismiss", async (string id, IMediator mediator) =>
        {
            var command = new DismissReminderCommand(id);

            var success = await mediator.Send(command);

            if (!success)
            {
                return Results.NotFound();
            }

            return Results.NoContent();
        });
    }
}