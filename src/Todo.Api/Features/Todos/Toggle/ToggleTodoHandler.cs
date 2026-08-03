using MediatR;
using MongoDB.Entities;
using Todo.Api.Common;
using Todo.Api.Entities;

namespace Todo.Api.Features.Todos.Toggle
{
    public class ToggleTodoHandler : IRequestHandler<ToggleTodoCommand, bool>
    {
        private readonly IMediator _mediator;

        public ToggleTodoHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<bool> Handle(ToggleTodoCommand request, CancellationToken cancellationToken)
        {
            // Tìm kiếm đối tượng cần đảo trạng thái
            var item = await DB.Find<TodoItem>().OneAsync(request.Id, cancellationToken);

            if (item == null) return false;

            // Đảo ngược trạng thái hiện tại
            item.IsCompleted = !item.IsCompleted;
            await item.SaveAsync(cancellation: cancellationToken);

            // Phát tín hiệu cho các dịch vụ khác (như Reminder) biết Todo đã đổi trạng thái
            await _mediator.Publish(new TodoStateChangedNotification
            {
                TodoId = request.Id,
                IsDeletedOrCompleted = item.IsCompleted // Thường nếu true thì bỏ reminder
            }, cancellationToken);

            return true;
        }
    }
}
