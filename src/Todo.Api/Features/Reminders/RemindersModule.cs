using AutoMapper;
using Carter;
using FluentValidation;
using MongoDB.Entities;
using Todo.Api.Features.Todos;

namespace Todo.Api.Features.Reminders
{
    public class RemindersModule : ICarterModule
    {
        private readonly IMapper _mapper;

        public RemindersModule(IMapper mapper)
        {
            _mapper = mapper;
        }

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var remGroup = app.MapGroup("/api/reminders");

            remGroup.MapGet("", async (string? state) =>
            {
                List<Reminder> reminders = new List<Reminder>();

                if (state == "pending")
                {
                    reminders = await DB.Find<Reminder>()
                    .Match(r => r.State == ReminderState.Pending)
                    .ExecuteAsync();
                }

                List<ReminderDto> result = _mapper.Map<List<ReminderDto>>(reminders);

                return Results.Ok(result);
            });

            // Reminder không hề có tiêu đề
            // Nên cần mói luôn cả todo mà bằng id mà thằng reminder này đang gắng vào.
            remGroup.MapGet("/upcoming", async (string? within) =>
            {
                var now = DateTime.UtcNow;

                var next24h = now.AddHours(24);

                List<TodoItem> items = new List<TodoItem>();

                items = await DB.Find<TodoItem>()
                .Match(t => t.IsCompleted == false)
                .Match(t => t.DueAt > now)
                .Match(t => t.DueAt < next24h)
                .ExecuteAsync();

                List<TodoDto> result = _mapper.Map<List<TodoDto>>(items);

                return Results.Ok(result);

            });

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


            }) ;

            remGroup.MapPatch("/{id}/dismiss", async (string id) => {
                var reminder = await DB.Find<Reminder>().OneAsync(id);

                if (reminder == null)
                {
                    return Results.NotFound();
                }

                reminder.State = ReminderState.Dismissed;

                await reminder.SaveAsync();

                return Results.NoContent();
            });
        }

    }
}
