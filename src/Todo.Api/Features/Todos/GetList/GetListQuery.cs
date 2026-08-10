using MediatR;

namespace Todo.Api.Features.Todos.GetList
{
    public record GetListQuery(string? filter) : IRequest<List<GetListResponse>>;
}
