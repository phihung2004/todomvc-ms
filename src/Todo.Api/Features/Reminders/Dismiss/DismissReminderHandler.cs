using MediatR;
using MongoDB.Entities;
using Todo.Api.Entities;

namespace Todo.Api.Features.Reminders.Dismiss;

public class DismissReminderHandler : IRequestHandler<DismissReminderCommand, bool>
{
    public async Task<bool> Handle(DismissReminderCommand request, CancellationToken cancellationToken)
    {
        bool isExist = await DB.Find<Reminder>()
                             .MatchID(request.Id)
                             .ExecuteAnyAsync(cancellationToken);

        if (!isExist)
        {
            return false;
        }

        await DB.Update<Reminder>()
                .MatchID(request.Id)
                .Modify(r => r.State, ReminderState.Dismissed)
                .ExecuteAsync(cancellationToken);

        return true;
    }
}