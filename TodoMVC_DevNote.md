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



services:

&#x09;image: mongodb:7

&#x09;hostname: mongodb

&#x09;container\_name: todo\_mongo

&#x09;port:

&#x09;- 27017:27017

&#x09;volumes:

&#x09;- mongo\_data:/data/db

volumes:

&#x09;mongo\_data:



###### cỏ bản bên trên là đã xong docker compose, sau đó thì nhớ các lệnh cơ bảng:

docker compose up : để chạy file compose, dính ở terminal, tắt terminal thì ngừng luôn

docker compose up -d: để chạy ngầm mà không dính ở terminal

docker compose down: XÓa hoàn toàn cái container của compose đang chạy



ok, vậy là đã cod DB trên doccker, test cái



**test:**

1. docker compose up -d

2\. MongoDB Compass > Tạo 1 DB + Collection Test > Tạo đại 1 document với data

3\. docker compose down

4\. check DB không kết nối được là dúng, vì nó chỉ nên chạy trên docker, tắt docker mà vẫn còn chạy là LMAO

5\. docker compose up -d để chạy ại container

6\. check lại MongoDB Compass vẫn còn data là đẹp trai.





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





#### **Làm riel:**



* tạo entity, rồi DTO + Mapping
* appsetting tạo connectionString vào trong MongoDB:"mongodb://localhost:27017"
* vào prgram cs, tạo builder, dùng builder.Configuration để lấy chuỗi connect với DB
* DB.InitAsync để có thể tạo 1 cái DB vào kết nối mongoDB
* cứ chạy vài cái method của DB để test đã kết nối lại vào DB hay chưa.





##### Mapping + Test

1. Mapp bằng AutoMapper rồi, giờ tìm cách để test viết 1 cái Minimal API dùng Entity trước, sau đó dùng DTO
2. Sẽ viết API trong program.cs trước, sau đó thì dừng Carter coi nó như lào.
3. sau đó thì taopj validate rồi test API mới viết.



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



CLient:

1. Là thằng trả về cá thông tin mà nó lấy được từ BE, dùng các Method của HttpClient để gọi API.
2. nên gói kiểu trả về là HttpResponseMessage vì nó có đầy đủ các thông tin BE trả về cả lỗi.
3. Vì hiện tại là map 1-1 , dự án nhỏ nên không cần, AI csobaro mốt nhiều gì đó thì nên MAP lại kiểu trả về là 1 DTO nào đó để mà gộp lại mà dùng.





### ============================================================ todo-app ===============================

Tạo dự án StandAlone Angular 20 bằng cmd. rồi mở bằng VSC



> npx @angular/cli@20 new todo-app --standalone



#### **Gọi về bằng service - RxJS**

#### **Và các compoent được phân nhẹ ra bằng NgRx - Component Store**



### **Đạn (Store) -> Súng (Component) -> Bóp Cò (HTML) -> Ra chiến trường (App.ts).**





**Đều có CLI để mà có thể tạo thay vì tọa bằng tay.**
Bước 1: Tạo file Service gọi API (nằm trong thư mục core)
---

ng g s core/todo-api



#### Bước 2: Tạo các Component cho UI (nằm trong thư mục features/todos)

ng g c features/todos/todo-list

ng g c features/todos/todo-item

ng g c features/todos/todo-input

ng g c features/todos/footer



#### Bước 3: Tạo file Store (NgRx ComponentStore)

ng g s features/todos/todos.store



Tọa file biến môi trường:

ng generate environments





#### Thứ tự cần code:



##### Service > Store > App.ts



#### **Note:**

1. Service ok, đã test địa bằng AI để gen AI để coi nó kết nnoois với BFF ok rồi.
2. App + COmponent + Store. Méo biết làm cái nào trước.
3. làm bắt đầu là store sẽ gọi service nền lmaf tiếp nó:
* &#x20;store dùng Component Store nên rất phức tạp với mình, base là State- kho chứa các biến cần thao tác
* Selector, lấy các data theo điều kiện mình muốn để nấu
* Updater: Thao tác chọc tay vào State để biến đổi kho
* Effect: Bùa, nhận tham số từ các Component (ts+html) để gọi Service + Dùng các Updater để nấu lại UI và data sau khi người dùng thao tác

4\. cấu trúc với cú pháp của các event, lấy property thì hơi lạ, với mấy thao tác hành vi của UI thì cần AI để mà nó chỉ hướng mới biết làm.

5\. CSS là quên gần hết rồi, nên dùng luôn cấu trúc mà TodoMVC có sắn.







Set up:

1. app.config.ts
2. app.component.ts/app.ts
3. folder core > todo-api.service.ts
4. folder feature/todos/components gồm:

   * footer
   * todo-input
   * todo-item
   * components/todos.store.ts

