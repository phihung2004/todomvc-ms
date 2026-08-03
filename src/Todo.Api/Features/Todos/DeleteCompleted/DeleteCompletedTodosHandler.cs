using MediatR;
using MongoDB.Entities;
using Todo.Api.Entities;

namespace Todo.Api.Features.Todos.DeleteCompleted
{
    public class DeleteCompletedTodosHandler : IRequestHandler<DeleteCompletedTodosCommand, bool>
    {
        public async Task<bool> Handle(DeleteCompletedTodosCommand request, CancellationToken cancellationToken)
        {
            // LOGIC CORE: 
            // Thay vì Find ra một List rồi gọi DeleteAllAsync() tốn 2 nhịp (1 nhịp kéo data về RAM, 1 nhịp xóa),
            // DB.DeleteAsync(...) sẽ đẩy thẳng câu lệnh điều kiện xuống MongoDB để xóa trực tiếp.     
            await DB.DeleteAsync<TodoItem>(i => i.IsCompleted == true, cancellation: cancellationToken);

            return true;
        }
    }
}
