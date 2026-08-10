using FluentValidation;

namespace Todo.Api.Features.Todos.Update
{
    // Tương tự các Validator khác.
    // Nó validate COmmand nên check kỹ, chứ nó không check cái request
    public class UpdateTodoCommandValidator : AbstractValidator<UpdateTodoCommand>
    {
        public UpdateTodoCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();

            RuleFor(x => x.Title)
                .NotEmpty()
                .MaximumLength(200);
        }
    }
}
