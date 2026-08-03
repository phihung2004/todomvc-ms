using MediatR;
using MongoDB.Entities;
using Todo.Api.Entities;

namespace Todo.Api.Features.Todos.GetById
{
    public class GetTodoByIdHandler : IRequestHandler<GetTodoByIdQuery, GetTodoByIdResponse?>
    {
        public async Task<GetTodoByIdResponse?> Handle(GetTodoByIdQuery request, CancellationToken cancellationToken)
        {
            var item = await DB.Find<TodoItem>().OneAsync(request.Id, cancellationToken);

            if (item == null)
            {
                return null;
            }

            // Map tay, hơi khẩm dô nhưng thôi, ¯\_(ツ)_/¯
            return new GetTodoByIdResponse(
                item.ID,
                item.Title,
                item.IsCompleted,
                item.CreateAt,
                item.DueAt
            );
        }
    }
}
