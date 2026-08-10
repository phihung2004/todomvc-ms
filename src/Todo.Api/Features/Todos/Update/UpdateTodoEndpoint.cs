using Carter;
using MediatR;

namespace Todo.Api.Features.Todos.Update;

public class UpdateTodoEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // Lý do dùng Command mà không dùng Request không thôi
        // request có thể kèm theo id,filter ở header, không gói hết vào body được
        // nên cần có 1 thằng gom lại hết toàn bộ.
        app.MapPut("/api/todos/{id}", async (string id, UpdateTodoRequest request, IMediator mediator) =>
        {
            var command = new UpdateTodoCommand(
                id,
                request.Title,
                request.IsCompleted,
                request.DueAt
            );

            // khúc này sẽ di qua cái validation mà mình đã khai báo trong Program cs
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