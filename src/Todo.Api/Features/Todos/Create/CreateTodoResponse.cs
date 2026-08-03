namespace Todo.Api.Features.Todos.Create
{
    public record CreateTodoResponse
    (
        string Id,
        string Title,
        DateTime CreateAt,
        DateTime? DueAt,
        bool IsCompleted
    );
}
