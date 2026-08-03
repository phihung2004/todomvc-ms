namespace Todo.Api.Features.Todos.Update
{
    public record UpdateTodoRequest(string Title, bool IsCompleted, DateTime? DueAt);
}
