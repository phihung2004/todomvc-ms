using MongoDB.Entities;

namespace Todo.Api.Entities
{
    // Y rang trong file anh Cường cho
    public class TodoItem : Entity
    {
        public string Title { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreateAt { get; set; }
    }
}
