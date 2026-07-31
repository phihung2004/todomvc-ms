using Carter;
using Todo.Bff.Extensions;

namespace Todo.Bff.Features.Todos.GetById
{
    public class GetByIdEndpoint : ICarterModule
    {
        // Sẽ cần nấu để gói MetaData gửi lên FE
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var todoGroup = app.MapGroup("/bff/todos");

            todoGroup.MapGet("/{id}", async (string id, TodoApiClient client) =>
            {
                var response = await client.GetTodoByIdAsync(id);

                //var rawContent = await response.Content.ReadAsStringAsync();
                //return Results.Content(rawContent, "application/json", statusCode: (int)response.StatusCode);
                return await response.ToResultAsync();
            });
        }
    }
}
