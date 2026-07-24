using Carter;
using Todo.Bff.Client;

var builder = WebApplication.CreateBuilder(args);


// Typed client:
builder.Services.AddHttpClient<TodoApiClient>(c =>
    c.BaseAddress = new Uri(builder.Configuration["TodoApi:BaseUrl"]!)); // http://localhost:5200

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