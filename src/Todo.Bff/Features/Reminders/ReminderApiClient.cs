namespace Todo.Bff.Features.Reminders
{
    // vẫn gọi về phía Api bên dưới, nên cần cái port với đúng URL
    public class ReminderApiClient
    {
        private readonly HttpClient _client;

        public ReminderApiClient(HttpClient client) 
        {
            _client = client;
        }

        public async Task<HttpResponseMessage> GetPendingReminderAsync (string? state)
        {
            return await _client.GetAsync($"/api/reminders?state={state}");
        }

        public async Task<HttpResponseMessage> GetUpcomingReminderAsync (string? within)
        {
            return await _client.GetAsync($"/api/reminders/upcoming?within={within}");
        }

        public async Task<HttpResponseMessage> SnoozeReminderAsync(string id, SnoozeReminderRequest request)
        {
            // AsJson : nó sẽ biết Object/DTO thành JSON để gửi. Nếu muốn gửi về dạng Json
            return await _client.PatchAsJsonAsync($"/api/reminders/{id}/snooze", request);
        }

        public async Task<HttpResponseMessage> DismissReminderAsync(string id)
        {
            return await _client.PatchAsync($"/api/reminders/{id}/dismiss", null);
        }

    }
}
