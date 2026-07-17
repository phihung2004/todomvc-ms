//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.MapOpenApi();
//}

//app.UseHttpsRedirection();

//var summaries = new[]
//{
//    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
//};

//app.MapGet("/weatherforecast", () =>
//{
//    var forecast = Enumerable.Range(1, 5).Select(index =>
//        new WeatherForecast
//        (
//            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//            Random.Shared.Next(-20, 55),
//            summaries[Random.Shared.Next(summaries.Length)]
//        ))
//        .ToArray();
//    return forecast;
//})
//.WithName("GetWeatherForecast");

//app.Run();

//internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
//{
//    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
//}


using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Entities;
using Todo.Api.Entities;


// Khai báo builder, đầu tiên và nobrainer là nổ cái này
var builder = WebApplication.CreateBuilder(args);

var defaultConnectionString =
   builder.Configuration.GetValue<string>("ConnectionStrings:MongoDB");

//Console.WriteLine(defaultConnectionString);

var settings = MongoClientSettings.FromConnectionString(defaultConnectionString);

// Thêm service cho builder bên dưới
builder.Services.AddOpenApi();



// sau khi add service xong hết thì mới .Build()
var app = builder.Build();

// Méo biết, trên doc chỉ vậy
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//============================ API Section ======================

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

    return Results.Ok(items);
});

todoGroup.MapGet("/{id}", async (string id) =>
{
    // Nếu chỉ để như vầy thì nó đúng là đã find luôn, nhưng không gán vào đâu để show ra hết
    //await DB.Find<TodoItem>().OneAsync(id);


    // Như dưới này thì có thằng hứng là item, rồi return lại bên dưới băng Ok(item)
    var item = await DB.Find<TodoItem>().OneAsync(id);

    return item is not null ? Results.Ok(item) : Results.NotFound() ;
});

todoGroup.MapPost("", async (TodoItem todoitem) =>
{
    // Ver lỏ 1 =)))
    //await DB.SaveAsync(todoitem);

    todoitem.CreateAt = DateTime.Now;
    await DB.SaveAsync(todoitem);

    return Results.Created($"/api/todos/{todoitem.ID}", todoitem);
});

//============================ API Section ======================

await DB.InitAsync("todo_mongo",settings);

app.Run();