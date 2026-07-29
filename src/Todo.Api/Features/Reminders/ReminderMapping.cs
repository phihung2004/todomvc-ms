using AutoMapper;

namespace Todo.Api.Features.Reminders
{
    public class ReminderMapping : Profile
    {
        public ReminderMapping() 
        {
            // Từ DB gửi lên, entity thành DTO để hiện lên màn hình
            CreateMap<Reminder, ReminderDto>();
        }
    }
}
