using FluentValidation;

namespace Todo.Api.Features.Todos.Update
{
    // Tương tự các Validator khác.
    // Nó validate COmmand nên check kỹ, chứ nó không check cái request
    public class UpdateTodoCommandValidator : AbstractValidator<UpdateTodoCommand>
    {
        public UpdateTodoCommandValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title can't be empty")
                .MaximumLength(200).WithMessage("Title must 200 char MAX");

            RuleFor(x => x.DueAt).GreaterThan(DateTime.UtcNow)
                .When(x => x.DueAt.HasValue)
                .WithMessage("Due At can't be in the past");
        }
    }
}
