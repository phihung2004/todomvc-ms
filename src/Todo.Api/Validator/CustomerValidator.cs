using FluentValidation;
using Todo.Api.DTOs;

namespace Todo.Api.Validator
{
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

            RuleFor(x => x.Title).NotEmpty();
            RuleFor(x => x.Title).MaximumLength(200);
        }

        
    }

     public class UpdateTodoRequestValidator : AbstractValidator<UpdateTodoRequest>
    {
        public UpdateTodoRequestValidator()
        {          

            RuleFor(x => x.Title).NotEmpty();
            RuleFor(x => x.Title).MaximumLength(200);
        }
        
    }
}
