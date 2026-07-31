using Carter;
using Todo.Bff.Extensions;

namespace Todo.Bff.Features.Reminders.GetUpcoming
{
    public class GetUpcomingEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var reminderGroup = app.MapGroup("/bff/reminders");

            reminderGroup.MapGet("/upcoming", async (string? within, ReminderApiClient client) =>
            {
                var response = await client.GetUpcomingReminderAsync(within);

                return await response.ToResultAsync();
            });
        }
    }
}
