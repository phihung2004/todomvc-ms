using MongoDB.Entities;

namespace Todo.Api.Entities
{
    public class TodoItem : Entity
    {
        public string Title { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreateAt { get; set; }
    }
}
