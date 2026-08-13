using Carter;
using MediatR;
using MongoDB.Entities;
using Todo.Api.Common;
using Todo.Api.Entities;

namespace Todo.Api.Features.Todos.Delete
{
    public class DeleteTodoEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {  
            // Fix L5: check cái ID 24 ký tự, để không bị trùng với câu Completed.
            app.MapDelete("/api/todos/{id:length(24)}", async (string id, IMediator mediator) =>
            {
                // Tạo command từ request
                var command = new DeleteTodoCommand(id);

                // TÙy cái thao tác CRUD đang dùng mà nhận biến cho đúng.
                var isDeleted = await mediator.Send(command);

                if (!isDeleted)
                {
                    return Results.Problem(detail: "Todo Item Not found", statusCode: StatusCodes.Status404NotFound);
                }

                return Results.NoContent();

            });
        }
    }
}
