using Carter;
using MediatR;

namespace Todo.Api.Features.Statistics
{
    public class StatsOverviewEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/stats/overview", async (IMediator mediator) =>
            {
                var  query = new StatsOverviewQuery();

                var response = await mediator.Send(query);

                if (response == null) 
                {
                    return Results.Problem(detail: "Stats Overview Not found", statusCode: StatusCodes.Status404NotFound);
                }

                return Results.Ok(response);
            });
        }
    }
}
