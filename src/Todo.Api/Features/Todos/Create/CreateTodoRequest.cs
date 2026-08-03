namespace Todo.Api.Features.Todos.Create
{
    public record CreateTodoRequest
    (
        string Title, DateTime? DueAt
    );

}
