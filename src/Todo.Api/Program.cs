using Carter;
using FluentValidation;
using MongoDB.Driver;
using MongoDB.Entities;
using Todo.Api.Common.Behavior;
using Todo.Api.Entities;
using Todo.Api.Features.Reminders;
using Todo.Api.Features.Todos;


// Khai báo builder, đầu tiên và nobrainer là nổ cái này
var builder = WebApplication.CreateBuilder(args);

var defaultConnectionString =
   builder.Configuration.GetValue<string>("ConnectionStrings:MongoDB");

//Console.WriteLine(defaultConnectionString);

var settings = MongoClientSettings.FromConnectionString(defaultConnectionString);

// Thêm service cho builder bên dưới==================
builder.Services.AddOpenApi();
//builder.Services.AddAutoMapper(typeof(TodoMappings));
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddCarter();
builder.Services.AddHostedService<ReminderScanner>();
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    //cfg.NotificationPublisher = new MyCustomPublisher(); // this will be singleton
    //cfg.NotificationPublisherType = typeof(MyCustomPublisher); // this will be the ServiceLifetime
});


// sau khi add service xong hết thì mới .Build()
var app = builder.Build();

// Méo biết, trên doc chỉ vậy
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await DB.InitAsync("todo_mongo", settings);

// Thêm index để tìm nhanh hơn
await DB.Index<TodoItem>()
    .Key(t => t.IsCompleted, KeyType.Ascending)
    .Key(t => t.DueAt, KeyType.Ascending)
    .CreateAsync();

app.MapCarter();

app.Run();