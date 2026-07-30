//namespace Todo.Api.Features.Reminders
//{
//    public class ReminderDto
//    {
//        public string Id { get; set; }              // vì bên Entity tự tạo ID nên bển không viết, nhưng bên này là POLO > viết
//        public string TodoId { get; set; }          // ref TodoItem
//        public DateTime DueAt { get; set; }
//        public ReminderState State { get; set; }     // Pending | Snoozed | Dismissed
//        public DateTime? SnoozeUntil { get; set; }
//        public DateTime FiredAt { get; set; }        // lúc scanner phát hiện tới hạn
//    }

//    //public class SnoozeReminderRequest
//    //{
//    //    public int Minutes { get; set; }

//    //}
//}
