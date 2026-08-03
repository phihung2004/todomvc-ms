using MediatR;
using MongoDB.Driver;

namespace Todo.Api.Features.Todos.GetById
{
    //Dùng Query thay cho Command.Trả về Response có thể null.
    // Command: Dùng để thay đổi dữ liệu (Create, Update, Delete).
    // Query: Dùng để lấy dữ liệu (Get, GetAll).
    public record GetTodoByIdQuery(string Id) : IRequest<GetTodoByIdResponse?>;
}
