using Azure.Messaging.ServiceBus;
using Carter;
using FluentValidation;
using MongoDB.Driver;
using MongoDB.Entities;
using Todo.Api.Common.Behavior;
using Todo.Api.Common.Exceptions;
using Todo.Api.Entities;
using Todo.Api.Features.Reminders;
using Todo.Api.Features.Reminders.Delivery;
using Todo.Api.Features.Reminders.Messaging;


// Khai báo builder, đầu tiên và nobrainer là nổ cái này
var builder = WebApplication.CreateBuilder(args);

//var defaultConnectionString =
//   builder.Configuration.GetValue<string>("ConnectionStrings:MongoDB");
// [FIX M8]: Đọc cấu hình và chủ động văng lỗi rõ ràng nếu thiếu (Fail-Fast)
var defaultConnectionString = builder.Configuration.GetValue<string>("ConnectionStrings:MongoDB")
    ?? throw new InvalidOperationException("Thiếu cấu hình: ConnectionStrings:MongoDB");

var settings = MongoClientSettings.FromConnectionString(defaultConnectionString);
//Console.WriteLine(defaultConnectionString);

//var sbConnectionString = builder.Configuration.GetValue<string>("ConnectionStrings:ServiceBus");
// [FIX M8]: Tương tự cho ServiceBus
var sbConnectionString = builder.Configuration.GetValue<string>("ConnectionStrings:ServiceBus")
    ?? throw new InvalidOperationException("Thiếu cấu hình: ConnectionStrings:ServiceBus");
builder.Services.AddSingleton(new ServiceBusClient(sbConnectionString));

builder.Services.AddSingleton<ReminderScheduler>();
builder.Services.AddSingleton<ReminderStreamChannel>();

// ===== MỚI: Email delivery =====
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));

var emailOptions = builder.Configuration.GetSection("Email").Get<EmailOptions>()
    ?? throw new InvalidOperationException("Thiếu cấu hình: Email");

builder.Services
    .AddFluentEmail(emailOptions.FromEmail, emailOptions.FromName)
    .AddSmtpSender(new System.Net.Mail.SmtpClient(emailOptions.SmtpHost, emailOptions.SmtpPort)
    {
        EnableSsl = emailOptions.EnableSsl,
        Credentials = string.IsNullOrEmpty(emailOptions.SmtpUsername)
            ? null
            : new System.Net.NetworkCredential(emailOptions.SmtpUsername, emailOptions.SmtpPassword)
    });

builder.Services.AddScoped<IReminderDeliveryChannel, EmailReminderChannel>();
builder.Services.AddScoped<ReminderDeliveryService>();
// ===== hết phần MỚI =====


// Thêm service cho builder bên dưới==================
builder.Services.AddOpenApi();
//builder.Services.AddAutoMapper(typeof(TodoMappings));
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddCarter();
builder.Services.AddHostedService<ReminderScanner>(); // Nhả comment lại, vì vẫn dùng Công việc 2
builder.Services.AddHostedService<ReminderProcessor>();

builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddProblemDetails(); // Khai báo cho app biết sẽ dùng chuẩn ProblemDetails
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

app.UseExceptionHandler();

await DB.InitAsync("todo_mongo", settings);

// Thêm index để tìm nhanh hơn
await DB.Index<TodoItem>()
    .Key(t => t.IsCompleted, KeyType.Ascending)
    .Key(t => t.DueAt, KeyType.Ascending)
    .CreateAsync();
// Rây con đí sần, mới test cái đầu tiên đã nổ bà nó r. Cần có Unique Index để mà khắc phục vụ này.
await DB.Index<Reminder>()
    .Key(r => r.TodoId, KeyType.Ascending)
    .Option(o => o.Unique = true)
    .CreateAsync();

app.MapCarter();

app.Run();