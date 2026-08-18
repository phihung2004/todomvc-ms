using Azure.Messaging.ServiceBus;
using MongoDB.Entities;
using System.Text.Json;
using Todo.Api.Entities;
using Todo.Api.Features.Reminders.Delivery;
using Todo.Api.Features.Reminders.Messaging;

namespace Todo.Api.Features.Reminders
{
    public class ReminderProcessor : BackgroundService
    {
        private readonly ServiceBusClient _sbClient;
        private ServiceBusProcessor? _processor;
        private readonly ReminderStreamChannel _streamChannel;
        private readonly IServiceScopeFactory _scopeFactory;

        public ReminderProcessor(
            ServiceBusClient sbClient,
            ReminderStreamChannel streamChannel,
            IServiceScopeFactory scopeFactory)
        {
            _sbClient = sbClient;
            _streamChannel = streamChannel;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _processor = _sbClient.CreateProcessor("todo-due-reminders", new ServiceBusProcessorOptions());

            _processor.ProcessMessageAsync += HandleMessageAsync;
            _processor.ProcessErrorAsync += HandleErrorAsync;

            await _processor.StartProcessingAsync(stoppingToken);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task HandleMessageAsync(ProcessMessageEventArgs args)
        {
            var payload = JsonSerializer.Deserialize<ReminderDueMessage>(args.Message.Body);
            var todoId = payload!.TodoId;

            var todo = await DB.Find<TodoItem>()
               .MatchID(todoId)
               .ExecuteFirstAsync(args.CancellationToken);

            if (todo is null || todo.IsCompleted)
            {
                Console.WriteLine($"[ReminderProcessor] Skipped TodoId={todoId} (not found or already completed)");
                await args.CompleteMessageAsync(args.Message);
                return;
            }

            var existingReminder = await DB.Find<Reminder>()
                .Match(r => r.TodoId == todoId)
                .ExecuteFirstAsync(args.CancellationToken);

            Reminder reminder;

            if (existingReminder is null)
            {
                reminder = new Reminder
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

                    _streamChannel.NotifyNewReminder();
                }
                catch (MongoDB.Driver.MongoWriteException ex) when (ex.WriteError.Category == MongoDB.Driver.ServerErrorCategory.DuplicateKey)
                {
                    Console.WriteLine($"[ReminderProcessor] TodoId={todoId} already has a Reminder (duplicate key, safely ignored)");
                    await args.CompleteMessageAsync(args.Message);
                    return;
                }
            }
            else
            {
                Console.WriteLine($"[ReminderProcessor] TodoId={todoId} already has a Reminder, skipping create");
                reminder = existingReminder;
            }

            // MỚI: gọi delivery — idempotent check nằm trong ReminderDeliveryService,
            // nên gọi thoải mái ở đây mà không sợ gửi trùng mail cho Reminder đã Sent.
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var deliveryService = scope.ServiceProvider.GetRequiredService<ReminderDeliveryService>();
                await deliveryService.SendAsync(reminder, todo, args.CancellationToken);
            }
            catch (Exception ex)
            {
                // Không throw ra ngoài — lỗi delivery không được làm ASB retry lại việc tạo Reminder.
                Console.WriteLine($"[ReminderProcessor] Delivery error for TodoId={todoId}: {ex.Message}");
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