using AutoMapper;
using Carter;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Entities;
using Todo.Api.Features.Todos;

namespace Todo.Api.Features.Todos
{
    public class TodosModule : ICarterModule
    {
        private readonly IMapper _mapper;
        public TodosModule(IMapper mapper)
        {
            _mapper = mapper;
        }
        public void AddRoutes(IEndpointRouteBuilder app)
        {

            var todoGroup = app.MapGroup("/api/todos");

            todoGroup.MapGet("", async (string? filter) =>
            {
                List<TodoItem> items = new List<TodoItem>();

                if (filter == "all" || string.IsNullOrEmpty(filter))
                {
                    // cả 2 thằng bên dưới đều có thể lây toàn bộ về hết
                    //var item = await DB.Queryable<TodoItem>().ToListAsync();

                    items = await DB.Find<TodoItem>().ExecuteAsync();
                }
                else if (filter == "active")
                {
                    items = await DB.Queryable<TodoItem>().Where(i => i.IsCompleted == false).ToListAsync();

                }
                else if (filter == "completed")
                {
                    items = await DB.Queryable<TodoItem>().Where(i => i.IsCompleted == true).ToListAsync();
                }

                List<TodoDto> itemDto = _mapper.Map<List<TodoDto>>(items);

                return Results.Ok(itemDto);
            });

            todoGroup.MapGet("/{id}", async (string id) =>
            {
                // Nếu chỉ để như vầy thì nó đúng là đã find luôn, nhưng không gán vào đâu để show ra hết
                //await DB.Find<TodoItem>().OneAsync(id);


                // Như dưới này thì có thằng hứng là item, rồi return lại bên dưới băng Ok(item)
                var item = await DB.Find<TodoItem>().OneAsync(id);

                TodoDto itemDto = _mapper.Map<TodoDto>(item);

                return itemDto is not null ? Results.Ok(itemDto) : Results.NotFound();
            });

            todoGroup.MapPost("", async (CreateTodoRequest request, IValidator<CreateTodoRequest> validator) =>
            {
                // Ver lỏ 1 =)))
                //await DB.SaveAsync(todoitem);

                var validationResult = await validator.ValidateAsync(request);

                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                TodoItem item = _mapper.Map<TodoItem>(request);
                item.CreateAt = DateTime.Now;

                await DB.SaveAsync(item);

                TodoDto result = _mapper.Map<TodoDto>(item);
                return Results.Created($"/api/todos/{result.Id}", result);
            });

            todoGroup.MapPut("/{id}", async (string id, UpdateTodoRequest request, IValidator<UpdateTodoRequest> validator) =>
            {
                var validationResult = await validator.ValidateAsync(request);

                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }


                bool isExist = await DB.Find<TodoItem>().MatchID(id).ExecuteAnyAsync();

                if (!isExist)
                {
                    return Results.NotFound();
                }

                await DB.Update<TodoItem>()
                        .MatchID(id)
                        .Modify(i => i.Title, request.Title)
                        .Modify(i => i.IsCompleted, request.IsCompleted)
                        .ExecuteAsync();

                return Results.NoContent();

            });

            todoGroup.MapPatch("/{id}/toggle", async (string id) =>
            {
                var item = await DB.Find<TodoItem>().OneAsync(id);

                if (item == null)
                {
                    return Results.NotFound();
                }

                item.IsCompleted = !item.IsCompleted;

                await item.SaveAsync();

                return Results.NoContent();

            });

            todoGroup.MapDelete("/{id}", async (string id) =>
            {
                var item = await DB.Find<TodoItem>().OneAsync(id);

                if (item == null)
                {
                    // Return lại theo kiểu thông thường, Notfound: một Body trống rỗng kèm mã 404
                    //return Results.NotFound();

                    return Results.Problem(detail: "Todo Item Not Found", statusCode: StatusCodes.Status404NotFound);
                }

                await item.DeleteAsync();

                return Results.NoContent();

            });

            todoGroup.MapDelete("/completed", async () =>
            {
                //List<TodoItem> item = await DB.Find<TodoItem>().Match(i => i.IsCompleted == true).ExecuteAsync();

                //await item.DeleteAllAsync();

                await DB.DeleteAsync<TodoItem>(i => i.IsCompleted == true);

                return Results.NoContent();
            });

        }
    }
}
