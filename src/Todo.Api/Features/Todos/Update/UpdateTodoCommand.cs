using MediatR;

namespace Todo.Api.Features.Todos.Update
{
    public record UpdateTodoCommand(
    string Id,
    string Title,
    bool IsCompleted,
    DateTime? DueAt
) : IRequest<bool>;
}
