using Carter;
using MongoDB.Entities;
using Todo.Api.Common;
using Todo.Api.Entities;

namespace Todo.Api.Features.Todos.GetById
{
    public class GetTodoByIdEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var todoGroup = app.MapGroup("/api/todos");

            todoGroup.MapGet("/{id}", async (string id) =>
            {
                // Nếu chỉ để như vầy thì nó đúng là đã find luôn, nhưng không gán vào đâu để show ra hết
                //await DB.Find<TodoItem>().OneAsync(id);


                // Như dưới này thì có thằng hứng là item, rồi return lại bên dưới băng Ok(item)
                var item = await DB.Find<TodoItem>().OneAsync(id);

                if (item == null)
                {
                    return Results.NotFound();
                }

                var response = new TodoResponse(
                    item.ID,
                    item.Title,
                    item.IsCompleted,
                    item.CreateAt,
                    item.DueAt
                );

                return Results.Ok(response);
            });
        }
    }
}
