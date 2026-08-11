using MediatR;
using MongoDB.Entities;
using Todo.Api.Entities;
using Todo.Api.Features.Reminders;

namespace Todo.Api.Features.Todos.Create
{
    public class CreateTodoHandler : IRequestHandler<CreateTodoCommand, CreateTodoResponse>
    {
        // DÙng để cho lên Schedule
        private readonly ReminderScheduler _reminderScheduler;

        public CreateTodoHandler(ReminderScheduler reminderScheduler)
        {
            _reminderScheduler = reminderScheduler;
        }

        public async Task<CreateTodoResponse> Handle(CreateTodoCommand request, CancellationToken cancellationToken)
        {
            var todoItem = new TodoItem
            {
                Title = request.Title,
                DueAt = request.DueAt,
                IsCompleted = false,
                CreateAt = DateTime.UtcNow
            };

            // Save Async ở đây thì nó cần 2 tham số, (null, cancellationToken) vấn được.
            // Xác định luôn mình đang cần thằng nào trong tham số.
            await todoItem.SaveAsync(cancellation: cancellationToken);

            // Chỉ những Todo có hẹn giờ mới cần schedule
            if (todoItem.DueAt.HasValue)
            {
                
                var sequenceNumber = await _reminderScheduler.ScheduleAsync(
                    todoItem.ID, todoItem.DueAt.Value, cancellationToken);

                // Lưu lại vé để Update/Toggle/Delete sau này có thể Cancel
                // Why Update thêm 1 cái nữa dưới này ?

                //Vì Dùng Entity, tạo rồi mới cho ID để nấu.
                todoItem.ReminderSequenceNumber = sequenceNumber;
                await DB.Update<TodoItem>()
                        .MatchID(todoItem.ID)
                        .Modify(i => i.ReminderSequenceNumber, sequenceNumber)
                        .ExecuteAsync(cancellationToken);

                Console.WriteLine($"[CreateTodoHandler] Đã schedule TodoId={todoItem.ID}, sequence={sequenceNumber}, DueAt={todoItem.DueAt:HH:mm:ss}");
            }

            return new CreateTodoResponse(
                Id: todoItem.ID,
                Title: todoItem.Title,
                CreateAt: todoItem.CreateAt,
                DueAt: todoItem.DueAt,
                IsCompleted: todoItem.IsCompleted                
            );

        }
    }

}
