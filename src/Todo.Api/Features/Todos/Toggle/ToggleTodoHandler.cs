using MediatR;
using MongoDB.Entities;
using Todo.Api.Common;
using Todo.Api.Entities;
using Todo.Api.Features.Reminders;

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
            item.CompletedAt = item.IsCompleted ? DateTime.UtcNow : null; // Biểu thức 3 ngôi nhìn cho nó pro =))

            // Lưu DB thằng vừa mới toggle
            // Để thằng ReminderProcessor soi DB có ngay kết quả chính xác nhất.
            await item.SaveAsync(cancellation: cancellationToken);

            // Hủy lịch, vì Todo đó đã xong, không cần báo bell
            if (item.IsCompleted)
            {
                if (item.ReminderSequenceNumber.HasValue)
                {
                    await _reminderScheduler.CancelAsync(item.ReminderSequenceNumber.Value, cancellationToken);
                    // Update field SequenceNumber thành null
                    await DB.Update<TodoItem>()
                            .MatchID(item.ID)
                            .Modify(i => i.ReminderSequenceNumber, (long?)null)
                            .ExecuteAsync(cancellationToken);
                }
            }
            else
            {
                // Chưa xong mà vẫn còn DueAt
                // Cần xóa các Reminder đang có, rồi tạo lại Lịch
                if (item.DueAt.HasValue)
                {
                    // Dọn rác
                    await DB.DeleteAsync<Reminder>(r => r.TodoId == item.ID, cancellation: cancellationToken);

                    // Gửi tin nhắn qua ASB (Nếu nổ ngay bây giờ, DB đã có IsCompleted = false rồi, an toàn!)
                    var newSequence = await _reminderScheduler.ScheduleAsync(item.ID, item.DueAt.Value, cancellationToken);

                    // Cập nhật lại SequenceNumber
                    await DB.Update<TodoItem>()
                            .MatchID(item.ID)
                            .Modify(i => i.ReminderSequenceNumber, newSequence)
                            .ExecuteAsync(cancellationToken);
                }
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
