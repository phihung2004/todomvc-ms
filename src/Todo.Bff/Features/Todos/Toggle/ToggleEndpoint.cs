using Carter;
using Todo.Bff.Extensions;

namespace Todo.Bff.Features.Todos.Toggle
{
    public class ToggleEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var todoGroup = app.MapGroup("/bff/todos");

            todoGroup.MapPatch("/{id}/toggle", async (string id, TodoApiClient client) =>
            {
                var response = await client.ToggleTodoAsync(id);
                return await response.ToResultAsync();

                //var rawContent = await response.Content.ReadAsStringAsync();

                //return Results.Content(rawContent, "application/json", statusCode: (int)response.StatusCode);

            });
        }
    }
}
