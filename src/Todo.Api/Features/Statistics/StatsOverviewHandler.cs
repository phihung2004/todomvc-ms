using MediatR;

namespace Todo.Api.Features.Statistics
{
    //public class StatsOverviewHandler : IRequestHandler<StatsOverviewQuery, StatsOverviewResponse>
    //{
    //    public Task<StatsOverviewResponse> Handle(StatsOverviewQuery request, CancellationToken cancellationToken)
    //    {
    //        var now = DateTime.UtcNow;
    //        var today = now.Date; // Ra số ngày (vd: Ngày 4 thì ra kiểu DateTime: 2026-08-04 00:00:00). Và nó sẽ đổi luôn phần giờ thành > 00:00:00

    //        // Vãi, Chủ nhật(0) > Thứ Hai(1) > Thứ 3(2) > ... > Thứ Bảy(6)
    //        // lấy Thứ 3(2) - Thứ 2(1) = 1 => Thứ 3 cách Thứ 2 1 ngày > ĐÚNG!
    //        // Nhưng còn Chủ Nhật thì đang bị bủ:
    //        // Chủ nhât (0) - Thứ 2(1) = -1 => Cái này là tiến về tương lai, lấy thứ 2 của tuần sau rồi.
    //        // Nên công thức là ((7 + X) % 7) => chia lấy dư, lấy phần thừa auto ra đúng phần cần
    //        // Ép kiểu về int để nó trừ nhau
    //        int daysToMonday = ((7 + ((int)today.DayOfWeek - (int)DayOfWeek.Monday)) % 7);
    //        var startOfWeek = today.AddDays(-1 *  daysToMonday); // nghịch âm lại để "lùi" ngày từ toán Add.

    //        var lastWeek = today.AddDays(-6);


    //        throw new NotImplementedException();
    //    }
    //}
}
