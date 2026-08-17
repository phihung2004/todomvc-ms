using FluentValidation;

namespace Todo.Api.Features.Todos.Create
{
    //  Giờ chuyển qua CQRP (Command query responsibility Segregation) thì mình sẽ validate Command thay vì Request vì mediator mình send cái command
    public class CreateTodoCommandValidator : AbstractValidator<CreateTodoCommand>
    {
        public CreateTodoCommandValidator()
        {
            //Mẫu trên doc
            //RuleFor(x => x.Surname).NotEmpty();
            //RuleFor(x => x.Forename).NotEmpty().WithMessage("Please specify a first name");
            //RuleFor(x => x.Discount).NotEqual(0).When(x => x.HasDiscount);
            //RuleFor(x => x.Address).Length(20, 250);
            //RuleFor(x => x.Postcode).Must(BeAValidPostcode).WithMessage("Please specify a valid postcode");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title can't be empty")
                .MaximumLength(200).WithMessage("Title must 200 char MAX");

            //RuleFor(x => x.DueAt).GreaterThan(DateTime.UtcNow)
            //    .When(x => x.DueAt.HasValue)
            //    .WithMessage("Due At can't be in the past LMAO");
        }


    }
}
