using Carter;
using Todo.Bff.Extensions;

namespace Todo.Bff.Features.Reminders.Dismiss
{
    public class DismissEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var reminderGroup = app.MapGroup("/bff/reminders");

            reminderGroup.MapPatch("/{id}/dismiss", async (string id, ReminderApiClient client) =>
            {
                var response = await client.DismissReminderAsync(id);

                return await response.ToResultAsync();
            });
        }
    }
}
