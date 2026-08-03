using MediatR;
using MongoDB.Entities;
using Todo.Api.Entities;

namespace Todo.Api.Features.Todos.Update;

public class UpdateTodoHandler : IRequestHandler<UpdateTodoCommand, bool>
{
    public async Task<bool> Handle(UpdateTodoCommand request, CancellationToken cancellationToken)
    {
        // Check xem có tồn tại trong DB không. Dùng ExecuteAnyAsync cho nhẹ vì chỉ cần lấy kết quả true/false, không kéo data thừa.
        bool isExist = await DB.Find<TodoItem>()
                             .MatchID(request.Id)
                             .ExecuteAnyAsync(cancellationToken);

        if (!isExist)
        {
            return false;
        }

        // Cập nhật trực tiếp các trường bị thay đổi dưới DB mà không cần lôi object lên. Tối ưu cực tốt cho performance.
        await DB.Update<TodoItem>()
                .MatchID(request.Id)
                .Modify(i => i.Title, request.Title)
                .Modify(i => i.IsCompleted, request.IsCompleted)
                .Modify(i => i.DueAt, request.DueAt)
                .ExecuteAsync(cancellationToken);

        return true;
    }
}