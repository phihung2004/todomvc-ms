using MediatR;
using MongoDB.Entities;
using Todo.Api.Entities;

namespace Todo.Api.Features.Todos.GetList
{
    public class GetListHandler : IRequestHandler<GetListQuery, GetListResponse>
    {
        public async Task<GetListResponse> Handle(GetListQuery request, CancellationToken cancellationToken)
        {
            // Đếm trước tổng số lượng và số task chưa làm để nhét vào metadata trả về chung một lượt.
            // Cách này giúp Frontend không phải gọi thêm API rời chỉ để lấy số liệu vẽ lên các bộ đếm.
            long totalCount = await DB.CountAsync<TodoItem>(cancellation: cancellationToken);
            long activeCount = await DB.CountAsync<TodoItem>(item => item.IsCompleted == false, cancellation: cancellationToken);

            List<TodoItem> items;

            // Chặn điều kiện ngay từ lúc chọc xuống DB bằng hàm Match để chỉ kéo những data thực sự cần thiết về RAM.
            // Tránh tình trạng lôi hàng ngàn record lên rồi mới viết code C# để lọc, dễ gây tràn bộ nhớ.
            if (request.filter == "active")
            {
                items = await DB.Find<TodoItem>()
                                .Match(i => i.IsCompleted == false)
                                .ExecuteAsync(cancellationToken);
            }
            else if (request.filter == "completed")
            {
                items = await DB.Find<TodoItem>()
                                .Match(i => i.IsCompleted == true)
                                .ExecuteAsync(cancellationToken);
            }
            else
            {
                // Trả về toàn bộ danh sách để làm fallback mặc định cho trường hợp gõ "all" hoặc gửi một chuỗi filter rỗng.
                items = await DB.Find<TodoItem>().ExecuteAsync(cancellationToken);
            }

            var responseList = items.Select(item => new TodoItemResponse(
                item.ID,
                item.Title,
                item.IsCompleted,
                item.CreateAt,
                item.DueAt
            )).ToList();

            return new GetListResponse(responseList, totalCount, activeCount);
        }
    }
}
