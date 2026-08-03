using Todo.Api.Entities;

namespace Todo.Api.Features.Reminders.GetList;

public record GetRemindersResponse(
    string Id,
    string TodoId,
    DateTime DueAt,
    ReminderState State,
    DateTime? SnoozeUntil,
    DateTime FiredAt
);