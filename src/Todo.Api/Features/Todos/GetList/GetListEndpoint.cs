using Carter;
using MediatR;

namespace Todo.Api.Features.Todos.GetList;

public class GetListEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/todos", async (string? filter, IMediator mediator) =>
        {
            // Bắt tham số filter từ URL và ném ngay vào Query.
            // Việc phân tích chuỗi này để query DB là trách nhiệm của Handler, lớp API chỉ làm nhiệm vụ giao tiếp mạng.
            var query = new GetListQuery(filter);

            var response = await mediator.Send(query);

            return Results.Ok(response);
        });
    }
}