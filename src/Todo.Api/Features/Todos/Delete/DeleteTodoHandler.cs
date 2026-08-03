using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using MongoDB.Entities;
using Todo.Api.Common;
using Todo.Api.Entities;

namespace Todo.Api.Features.Todos.Delete
{
    public class DeleteTodoHandler : IRequestHandler<DeleteTodoCommand, bool>
    {
        // ạo Mediator dùng để bắn qua bên Reminder để nó dismiss
        private readonly IMediator _mediator;

        public DeleteTodoHandler(IMediator mediator) { 
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteTodoCommand request, CancellationToken cancellationToken)
        {
            var item = await DB.Find<TodoItem>().OneAsync(request.Id, cancellationToken);

            if (item == null) return false; // Trả về false để Endpoint biết là 404 Not Found

            await item.DeleteAsync(cancellation: cancellationToken);

            // dùng thằng Meiator để mà hú event để bên Reminderchinhr lại reminder item mỗi khi todo bị xóa
            await _mediator.Publish(new TodoStateChangedNotification
            {
                TodoId = request.Id,
                IsDeletedOrCompleted = true
            }, cancellationToken);

            return true;

        }
    }
}
