namespace Todo.Api.Common
{
    public record TodoResponse
    (         
        string Id, 
         string Title, 
         bool IsCompleted, 
         DateTime CreateAt, 
         DateTime? DueAt 
    );
}
