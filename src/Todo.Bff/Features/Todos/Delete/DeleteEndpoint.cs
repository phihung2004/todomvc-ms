using Carter;
using Todo.Bff.Extensions;

namespace Todo.Bff.Features.Todos.Delete
{
    public class DeleteEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var todoGroup = app.MapGroup("/bff/todos");

            todoGroup.MapDelete("/{id}", async (string id, TodoApiClient client) =>
            {
                var response = await client.DeleteTodoAsync(id);
                return await response.ToResultAsync();

                //var rawContent = await response.Content.ReadAsStringAsync();

                // Trả về siu ngu, tự xác định lỗi rồi trả về từng cái.
                // Mình nghĩ là lấy đúng mã lõi mà nó nổ r gửi về là đc
                // CÓ thể không quy định trước như 2 bro kia nhưng v cho basic.
                //return Results.Content(rawContent, "application/json", statusCode: (int)response.StatusCode);
            });
        }
    }
}
