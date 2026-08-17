using Carter;
using Todo.Bff.Extensions; // Import cái extension Proxy xịn xò của ông

namespace Todo.Bff.Features.Statistics.Overview
{
    public class StatsOverviewEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map đường dẫn gốc cho Feature này
            var statsGroup = app.MapGroup("/bff/stats");

            statsGroup.MapGet("/overview", async (StatsApiClient client) =>
            {
                // 1. Gọi client lấy dữ liệu từ Todo.Api
                var response = await client.GetStatsOverviewAsync();

                // 2. Proxy (chuyển tiếp) thẳng cục JSON và StatusCode về cho FE (Angular)
                return await response.ToResultAsync();
            })
            .WithName("GetStatsOverview");
        }
    }
}