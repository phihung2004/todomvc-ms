using Carter;
using FluentValidation;
using MongoDB.Entities;
using Todo.Api.Entities;

namespace Todo.Api.Features.Todos.Create
{
    public record CreateTodoRequest
    (
        string Title, DateTime? DueAt
    );

    public record CreateTodoResponse
    (
        string Id,
        string Title,
        DateTime CreateAt,
        DateTime? DueAt,
        bool IsCompleted
    );

    public class CreateTodoRequestValidator : AbstractValidator<CreateTodoRequest>
    {
        public CreateTodoRequestValidator()
        {
            //Mẫu trên doc
            //RuleFor(x => x.Surname).NotEmpty();
            //RuleFor(x => x.Forename).NotEmpty().WithMessage("Please specify a first name");
            //RuleFor(x => x.Discount).NotEqual(0).When(x => x.HasDiscount);
            //RuleFor(x => x.Address).Length(20, 250);
            //RuleFor(x => x.Postcode).Must(BeAValidPostcode).WithMessage("Please specify a valid postcode");

            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.DueAt).GreaterThan(DateTime.UtcNow)
                .When(x => x.DueAt.HasValue)
                .WithMessage("Due At can't be in the past");
        }


    }

    public class CreateTodoEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var todoGroup = app.MapGroup("/api/todos");

            todoGroup.MapPost("", async (CreateTodoRequest request, IValidator<CreateTodoRequest> validator) =>
            {
                // Ver lỏ 1 =)))
                //await DB.SaveAsync(todoitem);

                var validationResult = await validator.ValidateAsync(request);

                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                // Tại vì thằng TodoItem mới có ID nên phải map ngược lại tạo mới nó, để lấy ID
                // Xong rồi thằng Response mới có được ID mà trả về FE
                var item = new TodoItem
                {
                    Title = request.Title,
                    DueAt = request.DueAt,
                    CreateAt = DateTime.UtcNow,
                    IsCompleted = false,
                };

                await DB.SaveAsync(item);

                // thứ tự truyền vào phải đúng với thứ tự của Record
                var response = new CreateTodoResponse(item.ID, item.Title, item.CreateAt, item.DueAt, item.IsCompleted);
                
                return Results.Created($"/api/todos/{response.Id}", response);
            });

        }
    }
}
