using MediatR;
using MongoDB.Entities;
using Todo.Api.Entities;
using Todo.Api.Features.Reminders;

namespace Todo.Api.Features.Todos.Update;

public class UpdateTodoHandler : IRequestHandler<UpdateTodoCommand, bool>
{
    private readonly ReminderScheduler _reminderScheduler;

    public UpdateTodoHandler(ReminderScheduler reminderScheduler)
    {
        _reminderScheduler = reminderScheduler;
    }

    public async Task<bool> Handle(UpdateTodoCommand request, CancellationToken cancellationToken)
    {
        // Đổi từ ExecuteAnyAsync sang lấy full object, vì cần DueAt CŨ để so sánh
        var existing = await DB.Find<TodoItem>()
                             .MatchID(request.Id)
                             .ExecuteFirstAsync(cancellationToken);

        if (existing is null)
        {
            return false;
        }

        await DB.Update<TodoItem>()
                .MatchID(request.Id)
                .Modify(i => i.Title, request.Title)
                .Modify(i => i.IsCompleted, request.IsCompleted)
                .Modify(i => i.DueAt, request.DueAt)
                .ExecuteAsync(cancellationToken);

        // Chỉ đụng ASB khi DueAt thật sự đổi giá trị — tránh cancel/reschedule vô ích khi chỉ đổi Title
        if (existing.DueAt != request.DueAt)
        {
            if (existing.ReminderSequenceNumber.HasValue)
            {
                await _reminderScheduler.CancelAsync(existing.ReminderSequenceNumber.Value, cancellationToken);
            }

            // FIX C2: Dọn sạch Reminder cũ của Todo này dưới DB.
            // Phải dọn TRƯỚC khi Schedule cái mới để tránh Race Condition với ASB.
            await DB.DeleteAsync<Reminder>(r => r.TodoId == request.Id, cancellation: cancellationToken);

            // Hẹn Schedule ASB lại bằng cái DueAt mới
            // Update luôn cái sequence number mới luôn
            if (request.DueAt.HasValue && !request.IsCompleted)
            {
                var sequenceNumber = await _reminderScheduler.ScheduleAsync(
                    request.Id, request.DueAt.Value, cancellationToken);

                await DB.Update<TodoItem>()
                        .MatchID(request.Id)
                        .Modify(i => i.ReminderSequenceNumber, sequenceNumber)
                        .ExecuteAsync(cancellationToken);

                Console.WriteLine($"[UpdateTodoHandler] Đã reschedule TodoId={request.Id}, sequence={sequenceNumber}, DueAt={request.DueAt:HH:mm:ss}");
            }
            else
            {
                // DueAt bị xóa (null) hoặc Todo được complete cùng lúc -> dọn vé cũ, không tạo vé mới
                await DB.Update<TodoItem>()
                        .MatchID(request.Id)
                        .Modify(i => i.ReminderSequenceNumber, (long?)null)
                        .ExecuteAsync(cancellationToken);
            }
        }

        return true;
    }
}




// Cũ
 //public async Task<bool> Handle(UpdateTodoCommand request, CancellationToken cancellationToken)
 //   {
 //       // Check xem có tồn tại trong DB không. Dùng ExecuteAnyAsync cho nhẹ vì chỉ cần lấy kết quả true/false, không kéo data thừa.
 //       bool isExist = await DB.Find<TodoItem>()
 //                            .MatchID(request.Id)
 //                            .ExecuteAnyAsync(cancellationToken);

 //       if (!isExist)
 //       {
 //           return false;
 //       }

 //       // Cập nhật trực tiếp các trường bị thay đổi dưới DB mà không cần lôi object lên. Tối ưu cực tốt cho performance.
 //       await DB.Update<TodoItem>()
 //               .MatchID(request.Id)
 //               .Modify(i => i.Title, request.Title)
 //               .Modify(i => i.IsCompleted, request.IsCompleted)
 //               .Modify(i => i.DueAt, request.DueAt)
 //               .ExecuteAsync(cancellationToken);

 //       return true;
 //   }