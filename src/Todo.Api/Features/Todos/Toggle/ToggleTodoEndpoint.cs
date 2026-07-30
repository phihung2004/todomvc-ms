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
            var todoGroup = app.MapGroup("/api/todos");

            todoGroup.MapPatch("/{id}/toggle", async (string id, IMediator mediator) =>
            {
                var item = await DB.Find<TodoItem>().OneAsync(id);

                if (item == null)
                {
                    return Results.NotFound();
                }

                item.IsCompleted = !item.IsCompleted;

                await item.SaveAsync();

                // thêm thằng Meiator để mà hú event để bên Reminderchinhr lại reminder item mỗi khi toggle todo 
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
