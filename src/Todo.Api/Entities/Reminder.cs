using Microsoft.Azure.Amqp.Framing;
using MongoDB.Entities;

namespace Todo.Api.Entities
{
    public class Reminder : Entity
    {
        public string TodoId { get; set; }          // ref TodoItem
        public DateTime DueAt { get; set; }
        public ReminderState State { get; set; }     // Pending | Snoozed | Dismissed
        public DateTime? SnoozeUntil { get; set; }
        public DateTime FiredAt { get; set; }        // lúc scanner phát hiện tới hạn

        public DeliveryStatus DeliveryStatus { get; set; } = DeliveryStatus.Pending;
        public string? Channel { get; set; }          // "Email", sau này có thể là list nếu multi-channel
        public DateTime? SentAt { get; set; }
        public DateTime? LastAttemptAt { get; set; }
        public string? ErrorMessage { get; set; }
        public int RetryCount { get; set; }
    }

    public enum ReminderState { Pending, Snoozed, Dismissed }

    public enum DeliveryStatus { Pending, Sent, Failed }



}
