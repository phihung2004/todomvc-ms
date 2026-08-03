using Carter;
using MediatR;
using MongoDB.Entities;
using Todo.Api.Common;
using Todo.Api.Entities;

namespace Todo.Api.Features.Todos.GetById
{
    public class GetTodoByIdEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/todos/{id}", async (string id, IMediator mediator) =>
            {
                var query = new GetTodoByIdQuery(id);

                var response = await mediator.Send(query);

                if (response == null)
                {
                    return Results.Problem(detail: "Todo Item Not Found", statusCode: StatusCodes.Status404NotFound);
                }

                return Results.Ok(response);
            });
        }
    }
}
