using Carter;
using Todo.Bff.Extensions;

namespace Todo.Bff.Features.Reminders.Snooze
{
    public class SnoozeEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var reminderGroup = app.MapGroup("/bff/reminders");

            reminderGroup.MapPatch("/{id}/snooze", async (string id, SnoozeReminderRequest request, ReminderApiClient client) =>
            {
                var response = await client.SnoozeReminderAsync(id, request);

                return await response.ToResultAsync();
            });
        }
    }
}
