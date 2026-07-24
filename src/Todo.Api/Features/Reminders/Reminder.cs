using MongoDB.Entities;

namespace Todo.Api.Features.Reminders
{
    public class Reminder : Entity
    {
        public string TodoId { get; set; }          // ref TodoItem
        public DateTime DueAt { get; set; }
        public ReminderState State { get; set; }     // Pending | Snoozed | Dismissed
        public DateTime? SnoozeUntil { get; set; }
        public DateTime FiredAt { get; set; }        // lúc scanner phát hiện tới hạn
    }

    public enum ReminderState { Pending, Snoozed, Dismissed }

}
