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
                await DB.Update<Reminder>()
                    .Match(r => r.TodoId == notification.TodoId)
                    .Modify(r => r.State, ReminderState.Dismissed)
                    .ExecuteAsync(cancellationToken);
            }
        }
    }
}
