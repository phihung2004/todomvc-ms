using Carter;
using MongoDB.Entities;
using Todo.Api.Entities;

namespace Todo.Api.Features.Todos.DeleteCompleted
{
    public class DeleteCompletedTodosEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var todoGroup = app.MapGroup("/api/todos");

            todoGroup.MapDelete("/completed", async () =>
            {
                //List<TodoItem> item = await DB.Find<TodoItem>().Match(i => i.IsCompleted == true).ExecuteAsync();

                //await item.DeleteAllAsync();

                await DB.DeleteAsync<TodoItem>(i => i.IsCompleted == true);

                return Results.NoContent();
            });

        }
    }
}
