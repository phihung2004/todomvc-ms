using Carter;
using FluentValidation;
using MediatR;
using MongoDB.Entities;
using Todo.Api.Entities;

namespace Todo.Api.Features.Todos.Create
{
    public class CreateTodoEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            //Nhiệm vụ giờ chỉ còn là lấy request. send command rồi nhận về từ Handler.
            app.MapPost("/api/todos", async (CreateTodoRequest request, IMediator mediator) =>
            {
                var command = new CreateTodoCommand(request.Title, request.DueAt);

                var response = await mediator.Send(command);

                return Results.Created($"/api/todos/{response.Id}", response);

                // Damn, 3 dòng, còn lại đưa cho bên kia hết r
            });
        }
    }
}

// Endpoint cũ

    //}

    //public class CreateTodoEndpoint : ICarterModule
    //{
    //    public void AddRoutes(IEndpointRouteBuilder app)
    //    {
    //        var todoGroup = app.MapGroup("/api/todos");

    //        todoGroup.MapPost("", async (CreateTodoRequest request, IValidator<CreateTodoRequest> validator) =>
    //        {
    //            // Ver lỏ 1 =)))
    //            //await DB.SaveAsync(todoitem
    //            //

    //            // Bởi vì bây giờ đang dùng cái validation Bahvior nên không cần dùng ValidateAsync cho mỗi cái endpoint nữa
    //            //var validationResult = await validator.ValidateAsync(request);

    //            //if (!validationResult.IsValid)
    //            //{
    //            //    return Results.ValidationProblem(validationResult.ToDictionary());
    //            //}

    //            // Tại vì thằng TodoItem mới có ID nên phải map ngược lại tạo mới nó, để lấy ID
    //            // Xong rồi thằng Response mới có được ID mà trả về FE
    //            var item = new TodoItem
    //            {
    //                Title = request.Title,
    //                DueAt = request.DueAt,
    //                CreateAt = DateTime.UtcNow,
    //                IsCompleted = false,
    //            };

    //            await DB.SaveAsync(item);

    //            // thứ tự truyền vào phải đúng với thứ tự của Record
    //            var response = new CreateTodoResponse(item.ID, item.Title, item.CreateAt, item.DueAt, item.IsCompleted);

    //            return Results.Created($"/api/todos/{response.Id}", response);
    //        });

    //    }
    //}