using MongoDB.Entities;
using Todo.Api.Entities;

namespace Todo.Api.Features.Reminders
{
    public class ReminderScanner : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // hàm Update trong Unity, nào có token hủy thì cút
            while (!stoppingToken.IsCancellationRequested)
            {
                await DoWork(stoppingToken);
                // Nghỉ 30 như yêu cầu
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }

        }

        private async Task DoWork(CancellationToken stoppingToken)
        {

            // Công việc 1: Tìm todo chưa có reminder, để mà tạo reminder

            var now = DateTime.UtcNow;

            // cần tìm các todo mà tụi nó đã trễ hạn. (DueAt bé hoặc bằng hiện tại => đã qua dueAt)
            // Kết quả: Danh sách các todo trễ hạn
            var overdueTodos = await DB.Find<TodoItem>()
                    .Match(todo => todo.IsCompleted == false && todo.DueAt <= now)
                    .ExecuteAsync(stoppingToken);

            // Nếu không scan thấy todo trễ hạn thì ngừng việc scan 
            if (overdueTodos.Any())
            {
                // đã có danh sách các todo trễ hạn, giờ thì lấy id của tụi nó.
                // Kết quả: Danh sách ID
                var overdueTodoIds = overdueTodos.Select(todo => todo.ID).ToList();


                // cần tìm danh sách ID của các Reminder từ danh sách id của các todo trễ hạn.
                // KQ: danh sách ID của các reminder
                var existingReminders = await DB.Find<Reminder>()
                    .Match(r => overdueTodoIds.Contains(r.TodoId))
                    .Project(p => p.Include(r => r.TodoId)) // Project là dùng để mà có thể DTO hóa lại kết quả ? 
                    .ExecuteAsync(stoppingToken);

                // Cần tìm danh sách các todo đã có reminder, để lọc ngược lại mấy thằng không có.
                // Từ Danh sách ID của các Reminder > lấy danh sách ID todo nó đang gắng vào
                // KQ: Danh sách ID các todo đã có Reminder
                var existingTodoIdsWithReminders = existingReminders.Select(r => r.TodoId).ToList();


                // Cần tìm các todo đã quá hạn mà không có reminder để mà từ đó thêm reminder vào.
                // KQ: tìm các todo không có reminder, để lấy id và thông tin cơ bản để mà tạo thêm 1 reminder từ
                var newReminders = overdueTodos
                    .Where(t => !existingTodoIdsWithReminders.Contains(t.ID)) // chọn mấy thằng todo chưa có reminder
                    .Select(t => new Reminder // lấy full todo ra, để lấy property của tụi nó để tạo mới Reminder.
                    {
                        TodoId = t.ID,
                        DueAt = t.DueAt.Value, // Do DueAt của Todo là nullable (DateTime?) nên phải lấy .Value
                        State = ReminderState.Pending,
                        FiredAt = now
                    }).ToList();

                if (newReminders.Any())
                {
                    await newReminders.SaveAsync(cancellation: stoppingToken);
                }
            }


            // Công việc 2: CHuyển mấy reminder đã snooze quá hạn từ Snoozed về Pending

            await DB.Update<Reminder>()
                .Match(r => r.SnoozeUntil <= now && r.State == ReminderState.Snoozed)
                .Modify(r => r.SnoozeUntil , null)
                .Modify(r => r.State , ReminderState.Pending)
                .Modify(r => r.FiredAt , now)
                .ExecuteAsync (stoppingToken);
        }
    }
}
