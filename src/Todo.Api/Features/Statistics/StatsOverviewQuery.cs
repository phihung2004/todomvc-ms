using MediatR;

namespace Todo.Api.Features.Statistics
{
    // Học bài ++. Đọc/Lấy Data thì gọi là Query, còn Command là Ghi/Sửa/Xóa
    public record StatsOverviewQuery() : IRequest<StatsOverviewResponse>;
   
}
