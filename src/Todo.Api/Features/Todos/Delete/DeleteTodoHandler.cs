using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using MongoDB.Entities;
using Todo.Api.Common;
using Todo.Api.Entities;
using Todo.Api.Features.Reminders;

namespace Todo.Api.Features.Todos.Delete
{
    public class DeleteTodoHandler : IRequestHandler<DeleteTodoCommand, bool>
    {
        // ạo Mediator dùng để bắn qua bên Reminder để nó dismiss
        private readonly IMediator _mediator;
        private readonly ReminderScheduler _reminderScheduler;

        public DeleteTodoHandler(IMediator mediator, ReminderScheduler reminderScheduler)
        {
            _mediator = mediator;
            _reminderScheduler = reminderScheduler;
        }

        public async Task<bool> Handle(DeleteTodoCommand request, CancellationToken cancellationToken)
        {
            var item = await DB.Find<TodoItem>().OneAsync(request.Id, cancellationToken);

            if (item == null) return false; // Trả về false để Endpoint biết là 404 Not Found


            // Todo: tạo 1 handle riêng chỉ làm cho việc đốt lịch
            // [FIX H2]: Hủy vé ASB trước khi Todo bị xóa
            if (item.ReminderSequenceNumber.HasValue)
            {
                await _reminderScheduler.CancelAsync(item.ReminderSequenceNumber.Value, cancellationToken);
            }

            await item.DeleteAsync(cancellation: cancellationToken);

            // dùng thằng Meiator để mà hú event để bên Reminder chỉnh lại reminder item mỗi khi todo bị xóa
            await _mediator.Publish(new TodoStateChangedNotification
            {
                TodoId = request.Id,
                IsDeletedOrCompleted = true
            }, cancellationToken);

            return true;

        }
    }
}
