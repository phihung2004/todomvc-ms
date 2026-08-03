using Carter;
using MediatR;

namespace Todo.Api.Features.Todos.Update;

public class UpdateTodoEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/todos/{id}", async (string id, UpdateTodoRequest request, IMediator mediator) =>
        {
            var command = new UpdateTodoCommand(
                id,
                request.Title,
                request.IsCompleted,
                request.DueAt
            );

            var success = await mediator.Send(command);

            // Handler trả về false nghĩa là query DB không ra item nào
            if (!success)
            {
                return Results.NotFound();
            }

            return Results.NoContent();
        });
    }
}