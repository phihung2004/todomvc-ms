using Carter;
using Todo.Bff.ApiClient;
using Todo.Bff.DTOs;
using Todo.Bff.Extensions;

namespace Todo.Bff.Modules
{
    // Thằng này hứng các request từ FE
    public class BffTodosModule : ICarterModule
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

            todoGroup.MapGet("/{id}", async (string id, TodoApiClient client) =>
            {
                var response = await client.GetTodoByIdAsync(id);

                //var rawContent = await response.Content.ReadAsStringAsync();
                //return Results.Content(rawContent, "application/json", statusCode: (int)response.StatusCode);
                return await response.ToResultAsync();
            });

            todoGroup.MapPost("", async (CreateTodoRequest request, TodoApiClient client) =>
            {
                var response = await client.CreateTodoAsync(request);
                //var rawContent = await response.Content.ReadAsStringAsync();

                //return Results.Content(rawContent, "application/json", statusCode: (int)response.StatusCode);
                return await response.ToResultAsync();

            });

            todoGroup.MapPut("/{id}", async (string id, UpdateTodoRequest request, TodoApiClient client) =>
            {
                var response = await client.UpdateTodoAsync(id, request);

                //var rawContent = await response.Content.ReadAsStringAsync();

                //return Results.Content(rawContent, "application/json", statusCode: (int)response.StatusCode);
                return await response.ToResultAsync();


            });

            todoGroup.MapPatch("/{id}/toggle", async (string id, TodoApiClient client) =>
            {
                var response = await client.ToggleTodoAsync(id);
                return await response.ToResultAsync();

                //var rawContent = await response.Content.ReadAsStringAsync();

                //return Results.Content(rawContent, "application/json", statusCode: (int)response.StatusCode);

            });

            todoGroup.MapDelete("/{id}", async (string id, TodoApiClient client) =>
            {
                var response = await client.DeleteTodoAsync(id);
                return await response.ToResultAsync();

                //var rawContent = await response.Content.ReadAsStringAsync();

                //return Results.Content(rawContent, "application/json", statusCode: (int)response.StatusCode);
            });

            todoGroup.MapDelete("/completed", async (TodoApiClient client) =>
            {
                var response = await client.DeleteCompledAsync();
                return await response.ToResultAsync();

                //var rawContent = await response.Content.ReadAsStringAsync();

                //return Results.Content(rawContent, "application/json", statusCode: (int)response.StatusCode);
            });
        }
    }
}
