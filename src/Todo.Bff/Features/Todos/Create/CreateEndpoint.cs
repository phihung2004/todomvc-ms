using Carter;
using Todo.Bff.Extensions;

namespace Todo.Bff.Features.Todos.Create
{
    // Không thêm Request ở đây,
    // VÌ thagnwf ApiCLient cũng dùng CreateTodoRequest nên là dùng chung, để bên Dto
    public class CreateEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var todoGroup = app.MapGroup("/bff/todos");

            todoGroup.MapPost("", async (CreateTodoRequest request, TodoApiClient client) =>
            {
                var response = await client.CreateTodoAsync(request);
                //var rawContent = await response.Content.ReadAsStringAsync();

                //return Results.Content(rawContent, "application/json", statusCode: (int)response.StatusCode);
                return await response.ToResultAsync();

            });

        }
    }
}
