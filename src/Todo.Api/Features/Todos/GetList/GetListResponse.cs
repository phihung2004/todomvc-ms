namespace Todo.Api.Features.Todos.GetList;

public record GetListResponse(
    string Id,
    string Title,
    bool IsCompleted,
    DateTime CreateAt,
    DateTime? DueAt
);