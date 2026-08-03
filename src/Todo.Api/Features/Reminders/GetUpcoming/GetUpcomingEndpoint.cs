using Carter;
using MediatR;

namespace Todo.Api.Features.Reminders.GetUpcoming;

public class GetUpcomingEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/reminders/upcoming", async (string? within, IMediator mediator) =>
        {
            var query = new GetUpcomingQuery(within);

            var result = await mediator.Send(query);

            return Results.Ok(result);
        });
    }
}