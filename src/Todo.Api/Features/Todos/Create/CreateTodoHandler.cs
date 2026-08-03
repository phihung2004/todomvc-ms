using MediatR;
using MongoDB.Entities;
using Todo.Api.Entities;

namespace Todo.Api.Features.Todos.Create
{
    public class CreateTodoHandler : IRequestHandler<CreateTodoCommand, CreateTodoResponse>
    {
        public async Task<CreateTodoResponse> Handle(CreateTodoCommand request, CancellationToken cancellationToken)
        {
            var todoItem = new TodoItem
            {
                Title = request.Title,
                DueAt = request.DueAt,
                IsCompleted = false,
                CreateAt = DateTime.UtcNow
            };

            // Save Async ở đây thì nó cần 2 tham số, (null, cancellationToken) vấn được.
            // Xác định luôn mình đang cần thằng nào trong tham số.
            await todoItem.SaveAsync(cancellation: cancellationToken);

            return new CreateTodoResponse(
                Id: todoItem.ID,
                Title: todoItem.Title,
                CreateAt: todoItem.CreateAt,
                DueAt: todoItem.DueAt,
                IsCompleted: todoItem.IsCompleted                
            );

        }
    }

}
