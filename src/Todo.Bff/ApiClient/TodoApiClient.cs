using Todo.Bff.DTOs;

namespace Todo.Bff.ApiClient
{
    // Gọi về Todo.Api nên map Url cho chuẩn
    public class TodoApiClient
    {
        private readonly HttpClient _client;

        public TodoApiClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<HttpResponseMessage> GetTodoAsync(string? filter)
        {
            return await _client.GetAsync($"/api/todos?filter={filter}");
        }

        public async Task<HttpResponseMessage> GetTodoByIdAsync(string id) 
        {
            return await _client.GetAsync($"/api/todos/{id}");        
        }

        public async Task<HttpResponseMessage> CreateTodoAsync(CreateTodoRequest request)
        {
            // Ver 1 
            //return await _client.GetFromJsonAsync<CreateTodoRequest>($"/api/todos/{request}");

            return await _client.PostAsJsonAsync("/api/todos",request);

        }

        public async Task<HttpResponseMessage> UpdateTodoAsync(string id, UpdateTodoRequest request) 
        {
            return await _client.PutAsJsonAsync($"/api/todos/{id}", request);            
        }

        public async Task<HttpResponseMessage> ToggleTodoAsync(string id)
        {
            return await _client.PatchAsync($"/api/todos/{id}/toggle", null);
        }

        public async Task<HttpResponseMessage> DeleteTodoAsync(string id)
        {
            return await _client.DeleteAsync($"/api/todos/{id}");
        }

        public async Task<HttpResponseMessage> DeleteCompledAsync()
        {
            return await _client.DeleteAsync("/api/todos/completed");
        }


    }
}
