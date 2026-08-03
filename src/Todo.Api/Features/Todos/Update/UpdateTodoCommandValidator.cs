using FluentValidation;

namespace Todo.Api.Features.Todos.Update
{
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
