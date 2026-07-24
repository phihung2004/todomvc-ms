using AutoMapper;

namespace Todo.Api.Features.Todos
{
    // Đặt tên file theo 1 ông anh nào đó trên StackOver Flow
    public class TodoMappings : Profile
    {
        public TodoMappings() 
        {
            // Map các luồng từ FE về BE
            // Lấy các trường chính yếu từ DTO map ngược về 1 Entity đầy đủ.
            CreateMap<CreateTodoRequest, TodoItem>();
            CreateMap<UpdateTodoRequest, TodoItem>();

            // Map các luồng từ BE lên FE
            CreateMap<TodoItem, TodoDto>();
        }
    }
}
