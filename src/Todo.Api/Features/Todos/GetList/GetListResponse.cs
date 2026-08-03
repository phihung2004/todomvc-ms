namespace Todo.Api.Features.Todos.GetList;

public record TodoItemResponse(
    string Id,
    string Title,
    bool IsCompleted,
    DateTime CreateAt,
    DateTime? DueAt
);

public record GetListResponse(
    List<TodoItemResponse> Items,
    long TotalCount,
    long ActiveCount
);