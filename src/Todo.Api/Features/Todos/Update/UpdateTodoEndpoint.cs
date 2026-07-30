using Carter;
using FluentValidation;
using MongoDB.Entities;
using Todo.Api.Entities;

namespace Todo.Api.Features.Todos.Update
{
    public record UpdateTodoRequest
       (
            string Title,
            bool IsCompleted,
            DateTime? DueAt
       );

    //public record UpdateTodoResponse
    //(
    //    string Id,
    //    string Title,
    //    DateTime CreateAt,
    //    DateTime? DueAt,
    //    bool IsCompleted
    //);

    public class UpdateTodoRequestValidator : AbstractValidator<UpdateTodoRequest>
    {
        public UpdateTodoRequestValidator()
        {

            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        }

    }

    public class UpdateTodoEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var todoGroup = app.MapGroup("/api/todos");

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
                        .Modify(i => i.DueAt, request.DueAt)
                        .ExecuteAsync();

                return Results.NoContent();

            });

        }
    }
}
