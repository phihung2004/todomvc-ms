using MediatR;
using MongoDB.Entities;
using Todo.Api.Common;
using Todo.Api.Entities;

namespace Todo.Api.Features.Reminders
{
    public class TodoStateChangedHandler : INotificationHandler<TodoStateChangedNotification>
    {
        public async Task Handle(TodoStateChangedNotification notification, CancellationToken cancellationToken)
        {
            if (notification.IsDeletedOrCompleted)
            {
                // Trên thì chỉ update lại thôi
                //await DB.Update<Reminder>()
                //    .Match(r => r.TodoId == notification.TodoId)
                //    .Modify(r => r.State, ReminderState.Dismissed)
                //    .ExecuteAsync(cancellationToken);

                // FIX H2: Xóa cái Reminder của những Todo đã bị xóa
                // thay vì chỉ đổi state thành Dismissed để chật Database.
                await DB.DeleteAsync<Reminder>(
                    r => r.TodoId == notification.TodoId,
                    cancellation: cancellationToken);
            }
        }
    }
}
