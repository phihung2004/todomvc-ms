using Carter;
using MongoDB.Entities;
using Todo.Api.Common;
using Todo.Api.Entities;


namespace Todo.Api.Features.Todos.GetList
{

    public record GetListResponse(
        List<TodoResponse>   Items, // vẫn trả về mớ item như thường, chỉ là gói thêm meta data theo
        long TotalCount,
        long ActiveCount
     );

    public class GetListEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var todoGroup = app.MapGroup("/api/todos");

            todoGroup.MapGet("", async (string? filter) =>
            {

                // THêm 2 thằng này để lấy từ DB trước, các MetaData cần
                long totalCount = await DB.CountAsync<TodoItem>(); // đếm Async, hàm mới, nhớ

                long activeCount = await DB.CountAsync<TodoItem>(item => item.IsCompleted == false);


                List<TodoItem> items = new List<TodoItem>();

                if (filter == "all" || string.IsNullOrEmpty(filter))
                {
                    // cả 2 thằng bên dưới đều có thể lây toàn bộ về hết
                    //var item = await DB.Queryable<TodoItem>().ToListAsync();

                    items = await DB.Find<TodoItem>().ExecuteAsync();
                }
                else if (filter == "active")
                {
                    items = await DB.Find<TodoItem>()
                                        .Match(i => i.IsCompleted == false)
                                        .ExecuteAsync();
                }
                else if (filter == "completed")
                {
                    items = await DB.Find<TodoItem>()
                    .Match(i => i.IsCompleted == true)
                    .ExecuteAsync();
                }

                var responseList = items.Select(item => new TodoResponse(
                    item.ID,
                    item.Title,
                    item.IsCompleted,
                    item.CreateAt,
                    item.DueAt
                )).ToList();

                var response = new GetListResponse(responseList, totalCount,activeCount);

                return Results.Ok(response);
            });

        }
    }
}
