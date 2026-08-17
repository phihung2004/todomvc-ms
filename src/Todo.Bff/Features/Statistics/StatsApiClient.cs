namespace Todo.Bff.Features.Statistics
{
    public class StatsApiClient
    {
        private readonly HttpClient _client;

        public StatsApiClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<HttpResponseMessage> GetStatsOverviewAsync()
        {
            // Gọi thẳng xuống đầu Endpoint của dự án Todo.Api
            return await _client.GetAsync("/api/stats/overview");
        }
    }
}