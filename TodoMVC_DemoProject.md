tải môi trường, tỉa docker, VS, MongoDB

Đọc yêu cầu anh Cường cho để code 1 trang cơ bản làm TodoMVC



Note lại quy trình và thứ tự để dev 1 project TodoMVC dùng Angular + ASP.NET Core Web API



### ============================================================ Thứ tự ===============================



1. **Setup:** Tạo DB bằng Docker để có chỗ lưu
2. **Todo.API:** Tạo dự án Todo.API để CRUD vào DB vừa tạo
3. **Todo.Bff:** Tạo Todo.Bff để call mirror vào Todo.API. Dùng cho FE bằng Angular 20 để gọi.
4. **todo-app:** Nấu FE bằng Angular 20



### ============================================================ Setup ===============================

#### **Docker:**

**Tạo file docker-compose.yml**

Tạo image của MongoDB đúng chuẩn. Check nếu anh Cường muốn chạy thì ảnh sẽ setup như thế nào.

Tại sao mà cái Images Mongo 7 t tạo mà chưa chạy mà nó vẫn lưu được local luôn :)))



là bước này đã có MongoDB nằm trên Docker với port 27017 thay vì dùng cái local



#### **MongoDB Compass:**

**Thế là méo dùng thằng này trên local à, vậy lúc chạy trên Docker thì như nào ?**





#### **VS 2022:**

Tải thư viện:

Auto mapper

Carte

FluentValidation

MongoDB.Entities



cần làm:

1. làm sao để không còn MongoDB local chạy và chiếm cổng 27017 của Docker





### ============================================================ Todo.Api ===============================

Tạo project ASP.NET core web API, bỏ tích controller để dùng đúng dạng Minimal API

Rồi học cách viết .gitignore để làm các thứ cơ bản để ném dự án lên github



#### Thứ tự cần code:



##### Folder Entities > DTOs > Mappings,Validators > Modules



Why?:

* Cần có Enitiy để Map Obj vào DB
* DTOs để che các trường quan trọng, chỉ expose các Proberty cần thiết
* Map để có thể cho code hiểu khi nào thằng DTO map vào Entity (Post, Update vì chỉ nhận những thằng cần bỏ vào DB, không nhận thêm), khi nào Entity map vào DTO (Khi Get, lấy full từ DB map vào DTO để nó hiện ra ngoài)
* Xác thực và kiểm tra đầu vào, để tránh các thông tin rác vào DB
* Viết Minimal API siêu nhanh và tiện, và dùng Carter để viết API ở 1 folder khác thày vì 1 nuồi trong Program.cs



Cần học:

1. Khai báo 1 Entities dùng MongoDB.Entities
2. Viết và khai báo DTO, chọn đúng các trường cần dùng
3. Tạo Profile để map DTO với Entity vừa tạo
4. Tạo 1 Vidatior dùng cho DTOs mới tạo
5. Làm dự án mẫu của MongoDB.Entities để mà có thể biết các LinQ cơ bản

6\. Khai báo và set up trong Program.cs

7\. Chạy thử và CRUD





reflect:

* Không làm gì trong appsetting hả ?
* Rồi cái này là đang dùng local, vậy còn cái images MongoDB 7 trên Docker làm gì ?

### ============================================================ Todo.Bff ===============================



#### Thứ tự cần code:



##### Copy DTOs > Clients > Modules



Why?:

* Bff cần biết DTOs mà Todo.API đang dùng để mà có thể gửi về hoặc nhận từ FE
* 1 thằng CLient để giao tiếp, nhìn mặt về Todo.API để nói chuyện
* 1 thằng Module để chìa các API lỗ tai để hứng các request được gọi từ FE về.



Cần học:

1. CLient - Viết API để Bff gọi về Todo.API riel bên dưới
2. Module - Dùng tiếp Carter để viết module các API lỗ tai để nghe call từ FE Angular gọi về.
3. Khai báo trong Program.cs. Cách setup CORS, Đăng ký httpClient để gọi về Todo.API





### ============================================================ todo-app ===============================

Tạo dự án StandAlone Angular 20 bằng cmd. rồi mở bằng VSC



Gọi về bằng service



#### Thứ tự cần code:



##### Service > Store > App.ts





Set up:

1. app.config.ts
2. app.component.ts/app.ts
3. folder core > todo-api.service.ts
4. folder feature/todos/components gồm:

   * footer
   * todo-input
   * todo-item
   * components/todos.store.ts

