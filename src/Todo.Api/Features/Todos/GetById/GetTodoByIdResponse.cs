namespace Todo.Api.Features.Todos.GetById
{
    public record GetTodoByIdResponse(
    string Id,
    string Title,
    bool IsCompleted,
    DateTime CreateAt,
    DateTime? DueAt
);
}
