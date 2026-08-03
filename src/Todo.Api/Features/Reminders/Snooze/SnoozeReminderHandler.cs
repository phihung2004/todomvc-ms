using MediatR;
using MongoDB.Entities;
using Todo.Api.Entities;

namespace Todo.Api.Features.Reminders.Snooze;

public class SnoozeReminderHandler : IRequestHandler<SnoozeReminderCommand, bool>
{
    // Cục này vẫn dùng để viết vào DB số phút mà người dùng nhập vào, nên bắt UI lại cho đẹp, BE bắt đc logic r
    public async Task<bool> Handle(SnoozeReminderCommand request, CancellationToken cancellationToken)
    {
        // Check tồn tại
        bool isExist = await DB.Find<Reminder>()
                             .MatchID(request.Id)
                             .ExecuteAnyAsync(cancellationToken);

        if (!isExist)
        {
            return false; // Trả false để búng ra 404
        }

        // Cập nhật State và cộng thêm phút vào thẳng DB
        await DB.Update<Reminder>()
                .MatchID(request.Id)
                .Modify(r => r.State, ReminderState.Snoozed)
                .Modify(r => r.SnoozeUntil, DateTime.UtcNow.AddMinutes(request.Minutes))
                .ExecuteAsync(cancellationToken);

        return true;
    }
}