using AutoMapper;
using Todo.Api.DTOs;
using Todo.Api.Entities;

namespace Todo.Api.Mappings
{
    // Đặt tên file theo 1 ông anh nào đó trên StackOver Flow
    public class MappingProfile : Profile
    {
        public MappingProfile() 
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
