using MediatR;
using MongoDB.Entities;
using Todo.Api.Entities;

namespace Todo.Api.Features.Reminders.GetList;

public class GetRemindersHandler : IRequestHandler<GetRemindersQuery, List<GetRemindersResponse>>
{
    public async Task<List<GetRemindersResponse>> Handle(GetRemindersQuery request, CancellationToken cancellationToken)
    {
        var reminders = new List<Reminder>();

        // Chỉ xử lý nếu state truyền lên là pending, còn lại mặc định mảng rỗng y như logic cũ
        if (request.State?.ToLower() == "pending")
        {
            reminders = await DB.Find<Reminder>()
                                .Match(r => r.State == ReminderState.Pending)
                                .ExecuteAsync(cancellationToken);
        }

        var result = reminders.Select(r => new GetRemindersResponse(
            r.ID,
            r.TodoId,
            r.DueAt,
            r.State,
            r.SnoozeUntil,
            r.FiredAt
        )).ToList();

        return result;
    }
}