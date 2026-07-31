using Carter;
using Todo.Bff.Extensions;

namespace Todo.Bff.Features.Todos.DeleteCompleted
{
    public class DeleteCompletedEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {

            // Mình có thể để thành 1 file static để dùng nhưng quá lười :)))
            var todoGroup = app.MapGroup("/bff/todos");

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
