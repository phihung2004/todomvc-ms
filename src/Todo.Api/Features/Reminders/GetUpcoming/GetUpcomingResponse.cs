namespace Todo.Api.Features.Reminders.GetUpcoming;

public record GetUpcomingResponse(
    string Id,
    string Title,
    bool IsCompleted,
    DateTime CreateAt,
    DateTime? DueAt
);