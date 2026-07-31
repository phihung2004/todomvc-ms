using Carter;
using Todo.Bff.Extensions;

namespace Todo.Bff.Features.Reminders.GetList
{
    public class GetListEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var reminderGroup = app.MapGroup("/bff/reminders");

            reminderGroup.MapGet("", async (string? state, ReminderApiClient client) =>
            {
                var response = await client.GetPendingReminderAsync(state);
                return await response.ToResultAsync();
            });
        }
    }
}
