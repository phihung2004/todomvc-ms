using Todo.Api.Entities;

namespace Todo.Api.Features.Reminders.Delivery
{
    public interface IReminderDeliveryChannel
    {
        string ChannelName { get; }
        Task<bool> SendAsync(Reminder reminder, TodoItem todo, CancellationToken ct);
    }

}
