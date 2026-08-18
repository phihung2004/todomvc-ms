using MongoDB.Entities;
using Todo.Api.Entities;

namespace Todo.Api.Features.Reminders.Delivery
{
    public class ReminderDeliveryService
    {
        private readonly IEnumerable<IReminderDeliveryChannel> _channels;
        private readonly ILogger<ReminderDeliveryService> _logger;

        public ReminderDeliveryService(
            IEnumerable<IReminderDeliveryChannel> channels,
            ILogger<ReminderDeliveryService> logger)
        {
            _channels = channels;
            _logger = logger;
        }

        public async Task SendAsync(Reminder reminder, TodoItem todo, CancellationToken ct)
        {
            // Idempotent check: đã gửi thành công rồi thì không gửi lại
            if (reminder.DeliveryStatus == DeliveryStatus.Sent)
            {
                _logger.LogInformation(
                    "ReminderId={ReminderId} đã ở trạng thái Sent, bỏ qua.", reminder.ID);
                return;
            }

            reminder.LastAttemptAt = DateTime.UtcNow;
            reminder.RetryCount += 1;

            var sentChannels = new List<string>();
            var allSucceeded = true;

            foreach (var channel in _channels)
            {
                var success = await channel.SendAsync(reminder, todo, ct);

                if (success)
                {
                    sentChannels.Add(channel.ChannelName);
                }
                else
                {
                    allSucceeded = false;
                    _logger.LogWarning(
                        "Channel {ChannelName} gửi thất bại cho ReminderId={ReminderId}",
                        channel.ChannelName, reminder.ID);
                }
            }

            reminder.Channel = string.Join(",", sentChannels);

            if (allSucceeded && sentChannels.Count > 0)
            {
                reminder.DeliveryStatus = DeliveryStatus.Sent;
                reminder.SentAt = DateTime.UtcNow;
                reminder.ErrorMessage = null;

                _logger.LogInformation(
        "Gửi thành công ReminderId={ReminderId}, TodoId={TodoId}, Channels={Channels}",
        reminder.ID, todo.ID, reminder.Channel);
            }
            else
            {
                reminder.DeliveryStatus = DeliveryStatus.Failed;
                reminder.ErrorMessage = sentChannels.Count > 0
                    ? "Một số channel gửi thất bại"
                    : "Tất cả channel đều gửi thất bại";
            }

            await DB.SaveAsync(reminder, cancellation: ct);
        }
    }
}