using MediatR;

namespace Todo.Api.Features.Reminders.Dismiss
{
    public record DismissReminderCommand(string Id) : IRequest<bool>;
}
