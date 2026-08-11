using MediatR;
using MongoDB.Entities;
using Todo.Api.Common;
using Todo.Api.Entities;
using Todo.Api.Features.Reminders;
using Todo.Api.Features.Todos.Toggle;

namespace Todo.Api.Features.Todos.Toggle
{
    public class ToggleTodoHandler : IRequestHandler<ToggleTodoCommand, bool>
    {
        private readonly IMediator _mediator;
        private readonly ReminderScheduler _reminderScheduler;

        public ToggleTodoHandler(IMediator mediator, ReminderScheduler reminderScheduler)
        {
            _mediator = mediator;
            _reminderScheduler = reminderScheduler;
        }

        public async Task<bool> Handle(ToggleTodoCommand request, CancellationToken cancellationToken)
        {
            var item = await DB.Find<TodoItem>().OneAsync(request.Id, cancellationToken);

            if (item == null) return false;

            item.IsCompleted = !item.IsCompleted;
            await item.SaveAsync(cancellation: cancellationToken);

            // Vừa complete (không phải uncomplete) và còn vé ASB đang chờ -> hủy, tránh rác message
            if (item.IsCompleted && item.ReminderSequenceNumber.HasValue)
            {
                await _reminderScheduler.CancelAsync(item.ReminderSequenceNumber.Value, cancellationToken);

                item.ReminderSequenceNumber = null;
                await DB.Update<TodoItem>()
                        .MatchID(item.ID)
                        .Modify(i => i.ReminderSequenceNumber, (long?)null)
                        .ExecuteAsync(cancellationToken);
            }

            await _mediator.Publish(new TodoStateChangedNotification
            {
                TodoId = request.Id,
                IsDeletedOrCompleted = item.IsCompleted
            }, cancellationToken);

            return true;
        }
    }
}



//private readonly IMediator _mediator;

//public ToggleTodoHandler(IMediator mediator)
//{
//    _mediator = mediator;
//}

//public async Task<bool> Handle(ToggleTodoCommand request, CancellationToken cancellationToken)
//{
//    // Tìm kiếm đối tượng cần đảo trạng thái
//    var item = await DB.Find<TodoItem>().OneAsync(request.Id, cancellationToken);

//    if (item == null) return false;

//    // Đảo ngược trạng thái hiện tại
//    item.IsCompleted = !item.IsCompleted;
//    await item.SaveAsync(cancellation: cancellationToken);

//    // Phát tín hiệu cho các dịch vụ khác (như Reminder) biết Todo đã đổi trạng thái
//    await _mediator.Publish(new TodoStateChangedNotification
//    {
//        TodoId = request.Id,
//        IsDeletedOrCompleted = item.IsCompleted // Thường nếu true thì bỏ reminder
//    }, cancellationToken);

//    return true;
//}
