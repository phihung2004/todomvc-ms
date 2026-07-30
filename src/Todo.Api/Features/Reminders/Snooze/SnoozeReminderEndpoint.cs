using Carter;
using FluentValidation;
using MongoDB.Entities;
using Todo.Api.Entities;

namespace Todo.Api.Features.Reminders.Snooze
{
    public record SnoozeReminderRequest(int Minutes);
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

    public class SnoozeReminderEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var remGroup = app.MapGroup("/api/reminders");

            remGroup.MapPatch("/{id}/snooze", async (string id, SnoozeReminderRequest request, IValidator<SnoozeReminderRequest> validator) => {

                var validationResult = await validator.ValidateAsync(request);

                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var reminder = await DB.Find<Reminder>().OneAsync(id);

                if (reminder == null)
                {
                    return Results.NotFound();
                }

                await DB.Update<Reminder>()
                .MatchID(id)
                .Modify(r => r.State, ReminderState.Snoozed)
                .Modify(r => r.SnoozeUntil, DateTime.UtcNow.AddMinutes(request.Minutes))
                .ExecuteAsync();

                return Results.NoContent();


            });

        }
    }
}
