using MediatR;
using MongoDB.Entities;
using Todo.Api.Entities;
using Todo.Api.Features.Reminders;

namespace Todo.Api.Features.Todos.DeleteCompleted
{
    public class DeleteCompletedTodosHandler : IRequestHandler<DeleteCompletedTodosCommand, bool>
    {
        private readonly ReminderScheduler _reminderScheduler;

        public DeleteCompletedTodosHandler(ReminderScheduler reminderScheduler)
        {
            _reminderScheduler = reminderScheduler;
        }

        public async Task<bool> Handle(DeleteCompletedTodosCommand request, CancellationToken cancellationToken)
        {
            // 1. Kéo nhẹ data về: Project thẳng về TodoItem nhưng chỉ gán ID và ReminderSequenceNumber
            var completedItems = await DB.Find<TodoItem>()
                .Match(i => i.IsCompleted == true)
                .Project(i => new TodoItem
                {
                    ID = i.ID,
                    ReminderSequenceNumber = i.ReminderSequenceNumber
                })
                .ExecuteAsync(cancellationToken);

            if (!completedItems.Any()) return true;

            var todoIds = completedItems.Select(x => x.ID).ToList();
            var sequencesToCancel = completedItems
                .Where(x => x.ReminderSequenceNumber.HasValue)
                .Select(x => x.ReminderSequenceNumber!.Value) // Trust me bro :))) Dùng ! để nói là mình biết nó sẽ méo null
                .ToList();

            // 2. [CORE FIX H1]: Hủy toàn bộ vé ASB của các Todo đã hoàn thành cùng một lúc
            if (sequencesToCancel.Any())
            {
                var cancelTasks = sequencesToCancel.Select(seq =>
                    _reminderScheduler.CancelAsync(seq, cancellationToken));
                await Task.WhenAll(cancelTasks);
            }

            // 3. Quét sạch bóng Reminder mồ côi trong DB
            await DB.DeleteAsync<Reminder>(r => todoIds.Contains(r.TodoId), cancellation: cancellationToken);

            // 4. Chốt hạ: Xóa hàng loạt Todo
            await DB.DeleteAsync<TodoItem>(i => todoIds.Contains(i.ID), cancellation: cancellationToken);

            return true;
        }
    }
}