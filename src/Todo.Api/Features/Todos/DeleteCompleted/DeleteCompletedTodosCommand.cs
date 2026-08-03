using MediatR;

namespace Todo.Api.Features.Todos.DeleteCompleted
{
    public record DeleteCompletedTodosCommand() : IRequest<bool>;
}
