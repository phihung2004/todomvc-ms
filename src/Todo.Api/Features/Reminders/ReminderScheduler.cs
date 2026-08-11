using Azure.Messaging.ServiceBus;
using System.Text.Json;
using Todo.Api.Features.Reminders.Messaging;

namespace Todo.Api.Features.Reminders
{
    // 	Gửi thư hẹn giờ tới ASB, nó sẽ nhận data khi Todo được tạo. 
    // RỒi sẽ gửi lên queue với thời gian là DueAt.
    public class ReminderScheduler
    {
        private readonly ServiceBusSender _sender;

        public ReminderScheduler(ServiceBusClient sbClient)
        {
            // Sender gắn cứng với 1 queue cụ thể, tạo 1 lần, dùng lại suốt vòng đời app
            _sender = sbClient.CreateSender("todo-due-reminders");
        }


        /// <summary>
        /// Lên lịch.
        /// </summary>
        /// <param name="todiId">Lấy đúng cái ID của Todo đó, vì Reminder cũng cần ID của Todo</param>
        /// <param name="dueAt">Ngày hẹn để nổ cái nhắc hẹn</param>
        /// <param name="ct">Để mà hủy request?</param>
        /// <returns>Làm 1 công việc là tạo 1 lịch hẹn sẽ tạo Reminder vào đúng DueAt</returns>
        public async Task<long> ScheduleAsync(string todiId, DateTime dueAt, CancellationToken ct)
        {
            var payload = new ReminderDueMessage(todiId);

            var message = new ServiceBusMessage(JsonSerializer.Serialize(payload))
            {
                ContentType = "application/json"
            };

            // Enqueue trễ, không phải gửi ngay — ASB tự giữ message tới đúng dueAt mới cho Processor thấy
            return await  _sender.ScheduleMessageAsync(message,dueAt, ct);
        }

        // Đốt lịch
        public async Task CancelAsync(long sequenceNumber, CancellationToken ct)
        {
            try
            {
                // Hủy cái lịch bằng cái sequence number
                await _sender.CancelScheduledMessageAsync(sequenceNumber, ct);
            }
            catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessageNotFound)
            {
                // DueAt nổ > Đã ném reminder lên Queue. Nên làm cục Cancle bên trên sẽ bị Exception là không tìm thấy > nó lên queue rồi
                Console.WriteLine($"[ReminderScheduler] Message sequence={sequenceNumber} không còn để cancel (có thể đã enqueue).");
            }
        }
    }
}
