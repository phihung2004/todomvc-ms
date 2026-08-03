using MediatR;

namespace Todo.Api.Features.Todos.Create
{
    // Kế thừa IRequest để chỉ định rõ Command này xử lý xong sẽ trả ra CreateTodoResponse
    public record CreateTodoCommand(string Title, DateTime? DueAt) : IRequest<CreateTodoResponse>;    
    
}
