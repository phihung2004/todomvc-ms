using System.Threading.Channels;

namespace Todo.Api.Features.Reminders.Messaging
{
    // Todo: Đang nhờ AI làm cái Push notification bằn cái channel bên dưới
    // Sẽ nghiên cứu và đổi thành SignalR để làm SSE
    public class ReminderStreamChannel
    {
        private readonly List<Channel<bool>> _subscribers = new();
        private readonly object _lock = new();

        public ChannelReader<bool> Subscribe()
        {
            var ch = Channel.CreateUnbounded<bool>();
            lock (_lock) { _subscribers.Add(ch); }
            return ch.Reader;
        }

        public void Unsubscribe(ChannelReader<bool> reader)
        {
            lock (_lock) { _subscribers.RemoveAll(s => s.Reader == reader); }
        }

        public void NotifyNewReminder()
        {
            lock (_lock)
            {
                foreach (var s in _subscribers) s.Writer.TryWrite(true);
            }
        }
    }
}