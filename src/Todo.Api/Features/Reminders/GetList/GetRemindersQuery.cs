using MediatR;

namespace Todo.Api.Features.Reminders.GetList
{
    public record GetRemindersQuery(string? State) : IRequest<List<GetRemindersResponse>>;
}
