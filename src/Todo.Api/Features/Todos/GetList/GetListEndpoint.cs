using Carter;
using MongoDB.Entities;
using Todo.Api.Common;
using Todo.Api.Entities;


namespace Todo.Api.Features.Todos.GetList
{
    public class GetListEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var todoGroup = app.MapGroup("/api/todos");

            todoGroup.MapGet("", async (string? filter) =>
            {
                List<TodoItem> items = new List<TodoItem>();

                if (filter == "all" || string.IsNullOrEmpty(filter))
                {
                    // cả 2 thằng bên dưới đều có thể lây toàn bộ về hết
                    //var item = await DB.Queryable<TodoItem>().ToListAsync();

                    items = await DB.Find<TodoItem>().ExecuteAsync();
                }
                else if (filter == "active")
                {
                    items = await DB.Find<TodoItem>()
                                        .Match(i => i.IsCompleted == false)
                                        .ExecuteAsync();
                }
                else if (filter == "completed")
                {
                    items = await DB.Find<TodoItem>()
                    .Match(i => i.IsCompleted == true)
                    .ExecuteAsync();
                }

                var responseList = items.Select(item => new TodoResponse(
                    item.ID,
                    item.Title,
                    item.IsCompleted,
                    item.CreateAt,
                    item.DueAt
                )).ToList();

                return Results.Ok(responseList);
            });

        }
    }
}
