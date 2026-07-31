using Carter;
using Todo.Bff.Extensions;

namespace Todo.Bff.Features.Todos.GetList
{
    // So, vì đã dùng Toresult nên ở BFF không cần map lại để lấy MetaData
    public class GetListEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var todoGroup = app.MapGroup("/bff/todos");

            todoGroup.MapGet("", async (string? filter, TodoApiClient client) =>
            {
                var response = await client.GetTodoAsync(filter);
                // Dùng cái extension, viết chung lại 1 class để dùng thay vì lặp lại, DRY
                return await response.ToResultAsync();

            });
        }
    }
}
