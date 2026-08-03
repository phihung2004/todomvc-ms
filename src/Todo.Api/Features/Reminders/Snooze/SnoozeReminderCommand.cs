using MediatR;

namespace Todo.Api.Features.Reminders.Snooze
{
    public record SnoozeReminderCommand(string Id, int Minutes) : IRequest<bool>;
}
