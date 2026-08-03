using MediatR;

namespace Todo.Api.Features.Todos.Toggle
{
    public record ToggleTodoCommand(string Id): IRequest<bool>;
    
}
