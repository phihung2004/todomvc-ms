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
            var todoGroup = app.MapGroup("/api/todos");

            todoGroup.MapDelete("/{id}", async (string id, IMediator mediator) =>
            {
                var item = await DB.Find<TodoItem>().OneAsync(id);

                if (item == null)
                {
                    // Return lại theo kiểu thông thường, Notfound: một Body trống rỗng kèm mã 404
                    //return Results.NotFound();

                    return Results.Problem(detail: "Todo Item Not Found", statusCode: StatusCodes.Status404NotFound);
                }

                await item.DeleteAsync();

                // thêm thằng Meiator để mà hú event để bên Reminderchinhr lại reminder item mỗi khi todo bị xóa
                await mediator.Publish(new TodoStateChangedNotification
                {
                    TodoId = id,
                    IsDeletedOrCompleted = true
                });

                return Results.NoContent();

            });
        }
    }
}
