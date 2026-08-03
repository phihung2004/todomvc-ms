using FluentValidation;

namespace Todo.Api.Features.Reminders.Snooze
{
    public class SnoozeReminderCommandValidator : AbstractValidator<SnoozeReminderCommand>
    {
        public SnoozeReminderCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Reminder ID is required.");

            // Giữ nguyên quả validation xịn sò của ông
            RuleFor(x => x.Minutes)
                .InclusiveBetween(10, 60)
                .WithMessage("Snooze minutes must be between 10 and 60");
        }
    }
}
