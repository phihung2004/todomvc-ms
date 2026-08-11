using System.Threading.Channels;

namespace Todo.Api.Features.Reminders.Messaging
{
    public class ReminderStreamChannel
    {
        private readonly Channel<bool> _channel = Channel.CreateUnbounded<bool>();

        public ChannelWriter<bool> Writer => _channel.Writer;
        public ChannelReader<bool> Reader => _channel.Reader;

        public void NotifyNewReminder()
        {
            // TryWrite không chờ, không throw nếu channel đầy — phù hợp vì đây chỉ là tín hiệu, không phải data quan trọng cần đảm bảo tới nơi
            _channel.Writer.TryWrite(true);
        }
    }
}