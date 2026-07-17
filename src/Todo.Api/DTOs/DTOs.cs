namespace Todo.Api.DTOs
{
    // Response DTO, dùng để trả về, lấy nguyên cục của DB trả về
    // Thường sẽ có full các trường cần thiết để hiện/trả ra ngoài FE
    // Cần Tittle để hiện
    //Cần bool để hiển thị là xong hay chưa
    //Cần ngày để mà sort
    public class TodoDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreateAt { get; set; }
    }

    // Khuôn để nhận request về từ FE, chỉ nhận đúng 1 cái Title
    // vì todo list chỉ cần đúng 1 cái title khi tạo là được
    // không cần đã check hay chưa, còn ngày thì tự thêm
    public class CreateTodoRequest 
    {
        public string Title { get; set; }

    }

    //Cũng là request ở phía FE
    //chỉ nhận đúng các trường cho phép update
    public class UpdateTodoRequest
    {
        public string Title { get; set; }
        public bool IsCompleted { get; set; }
    }

}
