using Carter;
using MediatR;
using MongoDB.Entities;
using Todo.Api.Common;
using Todo.Api.Entities;

namespace Todo.Api.Features.Todos.Toggle
{
    public class ToggleTodoEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPatch("/api/todos/{id}/toggle", async (string id, IMediator mediator) =>
            {
                var command = new ToggleTodoCommand(id);
                var success = await mediator.Send(command);

                if (!success)
                {
                    return Results.Problem(detail: "Todo Item Not Found", statusCode: StatusCodes.Status404NotFound);
                }

                return Results.NoContent();
            });
        }
    }
}
