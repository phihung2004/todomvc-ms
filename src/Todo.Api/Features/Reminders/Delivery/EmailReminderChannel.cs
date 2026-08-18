using FluentEmail.Core;
using Microsoft.Extensions.Logging;
using Todo.Api.Entities;

namespace Todo.Api.Features.Reminders.Delivery
{
    public class EmailReminderChannel : IReminderDeliveryChannel
    {
        private readonly IFluentEmail _fluentEmail;
        private readonly ILogger<EmailReminderChannel> _logger;

        public string ChannelName => "Email";

        public EmailReminderChannel(IFluentEmail fluentEmail, ILogger<EmailReminderChannel> logger)
        {
            _fluentEmail = fluentEmail;
            _logger = logger;
        }

        public async Task<bool> SendAsync(Reminder reminder, TodoItem todo, CancellationToken ct)
        {
            try
            {
                // TODO: thay bằng địa chỉ email thật của user khi có User/Account feature
                var toEmail = "kenvkl007@gmail.com";

                var subject = $"[TodoMVC] Nhắc hạn: {todo.Title}";
                var body =
                    $"Todo \"{todo.Title}\" đã tới hạn lúc {todo.DueAt:dd/MM/yyyy HH:mm}.\n" +
                    $"Vui lòng kiểm tra và xử lý.";

                var response = await _fluentEmail
                    .To(toEmail)
                    .Subject(subject)
                    .Body(body)
                    .SendAsync(ct);

                if (!response.Successful)
                {
                    _logger.LogWarning(
                        "Gửi email thất bại cho ReminderId={ReminderId}, TodoId={TodoId}. Lỗi: {Errors}",
                        reminder.ID, todo.ID, string.Join(", ", response.ErrorMessages));
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Exception khi gửi email cho ReminderId={ReminderId}, TodoId={TodoId}",
                    reminder.ID, todo.ID);
                return false;
            }
        }
    }
}