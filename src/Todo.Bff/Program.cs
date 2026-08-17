using Carter;
using Todo.Bff.Features.Reminders;
using Todo.Bff.Features.Statistics;
using Todo.Bff.Features.Todos;

var builder = WebApplication.CreateBuilder(args);

// Typed client:

// [FIX M8]: Kéo BaseUrl ra, kiểm tra null trước khi gán cho các HTTP Client
var todoApiBaseUrl = builder.Configuration["TodoApi:BaseUrl"]
    ?? throw new InvalidOperationException("Thiếu cấu hình: TodoApi:BaseUrl");

builder.Services.AddHttpClient<TodoApiClient>(c =>
    c.BaseAddress = new Uri(todoApiBaseUrl)); // http://localhost:5200
builder.Services.AddHttpClient<ReminderApiClient>(c =>
    c.BaseAddress = new Uri(todoApiBaseUrl)); // http://localhost:5200
builder.Services.AddHttpClient<StatsApiClient>(c =>
    c.BaseAddress = new Uri(todoApiBaseUrl));

// Thêm CORS cho `http://localhost:4200'
// Mẫu dùng tren doc, nó bảo là: " default CORS policy to all controller endpoints."
// Nó đang thiếu Header với Method (GET, PUT, POST,...), đang không nhận mấy cái đó. Angular gửi cũng vậy
// Thêm AllowAnyHeader với AllowAnyMethod để mà FE nso gọi vào
builder.Services.AddCors(option =>
    {
        option.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod();

        });
    });

builder.Services.AddCarter();

var app  = builder.Build();

// Xài CORS trước Carter
app.UseCors();

app.MapCarter();

app.Run();  