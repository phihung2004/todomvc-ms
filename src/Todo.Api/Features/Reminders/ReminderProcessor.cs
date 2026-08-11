using Azure.Messaging.ServiceBus;
using MongoDB.Entities;
using System.Text.Json;
using Todo.Api.Entities;
using Todo.Api.Features.Reminders.Messaging;

namespace Todo.Api.Features.Reminders
{
    public class ReminderProcessor : BackgroundService
    {
        private readonly ServiceBusClient _sbClient;
        private ServiceBusProcessor? _processor;
        private readonly ReminderStreamChannel _streamChannel;

        public ReminderProcessor(ServiceBusClient sbClient, ReminderStreamChannel streamChannel)
        {
            _sbClient = sbClient;
            _streamChannel = streamChannel;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _processor = _sbClient.CreateProcessor("todo-due-reminders", new ServiceBusProcessorOptions());

            _processor.ProcessMessageAsync += HandleMessageAsync;
            _processor.ProcessErrorAsync += HandleErrorAsync;

            await _processor.StartProcessingAsync(stoppingToken);

            // ExecuteAsync return là stop luôn BackgroundService, nên phải treo ở đây
            // để processor (chạy nền riêng) còn cơ hội tiếp tục sống. 
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task HandleMessageAsync(ProcessMessageEventArgs args)
        {
            var payload = JsonSerializer.Deserialize<ReminderDueMessage>(args.Message.Body);
            // Ý nghĩa: "tôi (dev) biết chắc biến này không null tại đây, compiler đừng cảnh báo nữa".
            var todoId = payload!.TodoId;

            var todo = await DB.Find<TodoItem>()
               .MatchID(todoId)
               .ExecuteFirstAsync(args.CancellationToken);

            // Todo không tồn tại (đã bị xóa) hoặc đã hoàn thành trước khi tới hạn -> bỏ qua
            if (todo is null || todo.IsCompleted)
            {
                Console.WriteLine($"[ReminderProcessor] Skipped TodoId={todoId} (not found or already completed)");
                await args.CompleteMessageAsync(args.Message);
                return;
            }

            // Check đã có Reminder cho Todo này chưa, tránh tạo trùng nếu message bị xử lý lại
            var alreadyExists = await DB.Find<Reminder>()
                .Match(r => r.TodoId == todoId)
                .ExecuteAnyAsync(args.CancellationToken);

            if (!alreadyExists)
            {
                var reminder = new Reminder
                {
                    TodoId = todoId,
                    DueAt = todo.DueAt!.Value,
                    State = ReminderState.Pending,
                    FiredAt = DateTime.UtcNow
                };

                try
                {
                    await reminder.SaveAsync(cancellation: args.CancellationToken);
                    Console.WriteLine($"[ReminderProcessor] Created Reminder for TodoId={todoId} at {DateTime.Now:HH:mm:ss}");

                    // Hú FE qua SSE
                    _streamChannel.NotifyNewReminder();
                }
                catch (MongoDB.Driver.MongoWriteException ex) when (ex.WriteError.Category == MongoDB.Driver.ServerErrorCategory.DuplicateKey)
                {
                    Console.WriteLine($"[ReminderProcessor] TodoId={todoId} already has a Reminder (duplicate key, safely ignored)");
                }
            }
            else
            {
                Console.WriteLine($"[ReminderProcessor] TodoId={todoId} already has a Reminder, skipping");
            }

            await args.CompleteMessageAsync(args.Message);
        }

        private Task HandleErrorAsync(ProcessErrorEventArgs args)
        {
            Console.WriteLine($"[ReminderProcessor] Error: {args.Exception.Message}");
            return Task.CompletedTask;
        }
    }
}