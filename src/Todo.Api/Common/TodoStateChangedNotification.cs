using MediatR;

namespace Todo.Api.Common
{
    public class TodoStateChangedNotification : INotification
    {
        public string TodoId { get; set; }
        public bool IsDeletedOrCompleted { get; set; }
    }
}
