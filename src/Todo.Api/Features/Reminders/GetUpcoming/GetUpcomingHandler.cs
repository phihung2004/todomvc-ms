using MediatR;
using MongoDB.Entities;
using Todo.Api.Entities;

namespace Todo.Api.Features.Reminders.GetUpcoming;

public class GetUpcomingHandler : IRequestHandler<GetUpcomingQuery, List<GetUpcomingResponse>>
{
    // Query : Không ghi DB = Không cần Validator
    public async Task<List<GetUpcomingResponse>> Handle(GetUpcomingQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var next24h = now.AddHours(24);

        // Chỗ này không cần khai báo new List<TodoItem>() trước làm gì cho tốn RAM,
        // Hứng luôn kết quả từ DB là được.
        var items = await DB.Find<TodoItem>()
            .Match(t => t.IsCompleted == false)
            .Match(t => t.DueAt > now)
            .Match(t => t.DueAt < next24h)
            .ExecuteAsync(cancellationToken);

        var responses = items.Select(item => new GetUpcomingResponse(
            item.ID,
            item.Title,
            item.IsCompleted,
            item.CreateAt,
            item.DueAt
        )).ToList();

        return responses;
    }
}