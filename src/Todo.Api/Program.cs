using AutoMapper;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Entities;
using Todo.Api.DTOs;
using Todo.Api.Entities;
using Todo.Api.Mappings;


// Khai báo builder, đầu tiên và nobrainer là nổ cái này
var builder = WebApplication.CreateBuilder(args);

var defaultConnectionString =
   builder.Configuration.GetValue<string>("ConnectionStrings:MongoDB");

//Console.WriteLine(defaultConnectionString);

var settings = MongoClientSettings.FromConnectionString(defaultConnectionString);

// Thêm service cho builder bên dưới==================
builder.Services.AddOpenApi();
builder.Services.AddAutoMapper(typeof(MappingProfile));



// sau khi add service xong hết thì mới .Build()
var app = builder.Build();

// Méo biết, trên doc chỉ vậy
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//============================ API Section ======================

var todoGroup = app.MapGroup("/api/todos");

todoGroup.MapGet("", async(string ? filter, IMapper mapper)  =>
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

    List<TodoDto> itemDto = mapper.Map<List<TodoDto>>(items);

    return Results.Ok(itemDto);
});

todoGroup.MapGet("/{id}", async (string id, IMapper mapper) =>
{
    // Nếu chỉ để như vầy thì nó đúng là đã find luôn, nhưng không gán vào đâu để show ra hết
    //await DB.Find<TodoItem>().OneAsync(id);


    // Như dưới này thì có thằng hứng là item, rồi return lại bên dưới băng Ok(item)
    var item = await DB.Find<TodoItem>().OneAsync(id);

    TodoDto itemDto = mapper.Map<TodoDto>(item);

    return itemDto is not null ? Results.Ok(itemDto) : Results.NotFound() ;
});

todoGroup.MapPost("", async (CreateTodoRequest request, IMapper mapper) =>
{
    // Ver lỏ 1 =)))
    //await DB.SaveAsync(todoitem);

    TodoItem item = mapper.Map<TodoItem>(request);
    item.CreateAt = DateTime.Now;

    await DB.SaveAsync(item);

    TodoDto result = mapper.Map<TodoDto>(item);
    return Results.Created($"/api/todos/{result.Id}", result);
});

todoGroup.MapPut("/{id}", async (string id, UpdateTodoRequest request) => 
{
    bool isExist = await DB.Find<TodoItem>().MatchID(id).ExecuteAnyAsync();

    if(!isExist)
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

    if (item ==null)
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

//============================ API Section ======================

await DB.InitAsync("todo_mongo",settings);

app.Run();