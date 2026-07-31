using Carter;
using Todo.Bff.Extensions;

namespace Todo.Bff.Features.Todos.Update
{
    public class UpdateEndpoint : ICarterModule
    {
        // Vấn thế, UpdateTodoRequest dùng chung bên ApiCLient nên không khai trong này => DRY
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var todoGroup = app.MapGroup("/bff/todos");

            todoGroup.MapPut("/{id}", async (string id, UpdateTodoRequest request, TodoApiClient client) =>
            {
                var response = await client.UpdateTodoAsync(id, request);

                //var rawContent = await response.Content.ReadAsStringAsync();

                //return Results.Content(rawContent, "application/json", statusCode: (int)response.StatusCode);
                return await response.ToResultAsync();


            });
        }
    }
}
