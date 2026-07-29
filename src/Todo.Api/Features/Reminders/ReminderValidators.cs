using FluentValidation;

namespace Todo.Api.Features.Reminders
{
    public class SnoozeReminderRequestValidator : AbstractValidator<SnoozeReminderRequest>
    {
        public SnoozeReminderRequestValidator() 
        {
            // Validator của gà :)))
            //RuleFor(x => x.Minutes).GreaterThan(9).LessThan(61);

            // Có validation riêng cho vụ từ phút nào tới phút nào luôn
            RuleFor(x => x.Minutes)
                .InclusiveBetween(10, 60)
                .WithMessage("Snooze minutes must be between 10 and 60");
        } 
    }
}
