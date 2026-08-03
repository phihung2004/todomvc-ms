using MediatR;

namespace Todo.Api.Features.Reminders.GetUpcoming
{
    public record GetUpcomingQuery(string? Within) : IRequest<List<GetUpcomingResponse>>;
}
