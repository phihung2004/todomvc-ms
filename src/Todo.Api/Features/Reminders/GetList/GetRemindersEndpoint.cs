using Carter;
using MediatR;

namespace Todo.Api.Features.Reminders.GetList;

public class GetRemindersEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/reminders", async (string? state, IMediator mediator) =>
        {
            var query = new GetRemindersQuery(state);

            var result = await mediator.Send(query);

            return Results.Ok(result);
        });
    }
}