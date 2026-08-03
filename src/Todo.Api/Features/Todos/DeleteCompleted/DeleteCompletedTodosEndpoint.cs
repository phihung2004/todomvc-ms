using Carter;
using MediatR;
using MongoDB.Entities;
using Todo.Api.Entities;

namespace Todo.Api.Features.Todos.DeleteCompleted
{
    public class DeleteCompletedTodosEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {   
            app.MapDelete("/api/todos/completed", async (IMediator mediator) =>
            {
                var command = new DeleteCompletedTodosCommand();
                await mediator.Send(command);

                // Thành công thì nhả 204 NoContent
                return Results.NoContent();
            });

        }
    }
}
