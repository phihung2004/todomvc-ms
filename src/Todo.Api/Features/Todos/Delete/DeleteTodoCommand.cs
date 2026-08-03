using MediatR;

namespace Todo.Api.Features.Todos.Delete
{
    public record DeleteTodoCommand(string Id) : IRequest<bool>;
}
