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





### **RERP Pattern**



1. **Mindset vấn giữ ok như thường, thứ mình cần là Gom lại các DTO, Validator vào 1 cái endpoint rồi tách từ từ.**
2. **chỉ cần kế thừa từ ICarterModule là được, cái mớ trong module giống thì comment lại, vì nó y rang nhau.**
3. **Làm tời endpoint nào thì test endpoint đó, rồi làm lại hết.**



**MediaR:**



**Ở toggle todo, đê rkhi có thagnwf toggle thì nó hú qua bên reminder để reminder update thằnh dismiss**





### **DB Aggregation COde** 





Bước 1: Setup các mốc thời gian (Time anchors)

Trang bị sẵn vũ khí trước khi ra trận. Bro cần khai báo các mốc thời gian sau để làm điều kiện lọc cho DB:



Lấy thời gian hiện tại (nên dùng chuẩn UTC).



Lấy mốc "Hôm nay" (chỉ lấy ngày, cắt bỏ phần giờ phút giây).



Dùng công thức toán học tính lùi lại để tìm ra chính xác mốc "Thứ Hai của tuần này" (dùng cho thống kê trong tuần).



Tính lùi 7 ngày từ hôm nay để làm mốc cho biểu đồ.



Bước 2: Dàn trận các truy vấn đếm số (Scalar Counts)

Ở bước này, bro sẽ định nghĩa các câu lệnh đếm (CountAsync) nhưng tuyệt đối chưa dùng từ khóa await. Việc này giống như bro đang đưa order cho nhà bếp nhưng chưa bắt họ nấu ngay.



Total: Đếm tất cả, không cần điều kiện.



Active: Đếm các item có cờ hoàn thành là false.



Completed: Đếm các item có cờ hoàn thành là true.



Overdue: Kết hợp 2 điều kiện: chưa hoàn thành VÀ hạn chót (DueAt) nhỏ hơn thời gian hiện tại.



CompletedToday: Đã hoàn thành VÀ thời gian tạo (hoặc thời gian hoàn thành nếu bro có field đó) lớn hơn hoặc bằng mốc "Hôm nay".



CompletedThisWeek: Đã hoàn thành VÀ thời gian >= mốc "Thứ Hai".



Bước 3: Kích hoạt chạy song song \& Tính toán phụ

Dùng Task.WhenAll(...) và await nó. Đây là lúc bro ném tất cả các order ở Bước 2 xuống DB cùng một lúc để nó đếm song song, giúp giảm thời gian chờ.



Sau khi có kết quả, viết logic tính Completion Rate (Tỷ lệ hoàn thành). Lưu ý sống còn: Nhớ bọc điều kiện kiểm tra Total = 0 để chương trình không nổ tung vì lỗi chia cho số 0. Cẩn thận ép kiểu về số thực (double) trước khi chia.



Bước 4: Khai mở Aggregation Pipeline (Gom nhóm biểu đồ)

Đây là phần não to nhất. Bro cần mở ống .Aggregate() và cho dữ liệu đi qua 2 trạm lọc:



Trạm 1 - Lọc (Match): Chặn cửa, chỉ cho phép những Task ĐÃ hoàn thành VÀ nằm trong khoảng 7 ngày qua đi tiếp.



Trạm 2 - Gom nhóm (Group): Phân loại dữ liệu. Bro sẽ gom nhóm chúng bằng một key ẩn danh bao gồm 3 thông số: Năm, Tháng, Ngày. Cứ mỗi nhóm được tạo ra, bro gọi hàm đếm tổng số lượng phần tử của nhóm đó.



Cuối cùng, await và ép nó thành một List.



Bước 5: Đóng gói và Giao hàng (Mapping \& Response)

Lấy cái List vừa gom được ở Bước 4, dùng vòng lặp hoặc LINQ .Select() để map nó sang dạng DailyCountDto chuẩn mà FE yêu cầu (nhớ chuyển đổi các cụm Năm-Tháng-Ngày thành kiểu DateOnly cho đẹp).



Khởi tạo object StatsOverviewResponse, nhồi tất cả các con số đếm được ở Bước 3 và cái List biểu đồ vừa map xong vào, rồi return về cho Endpoint.



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





Note M6-7:

1. Scanner, them thằng reminder, virtical slice. them chức năng mới thì cũng cần can nhắc có cần thay đổi cấu trúc của dự án không
2. Làm xong M7 thì chuyển sang REPR pattern.
3. 





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



ng g c features/reminder-panel/notification-bell

ng g c features/notification-bell/reminder-panel







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







NOte M6-7 - ha ha, six seven\~

1. Nắm được cấu trúc của 1 Component Store, State > Selector > Updater > Effect
2. component store là cứ xong Service là chạy qua làm Store, đủ mấy cái bên trên
3. Xong là về viết từng COmponent ts,html với css của nó là được









Set up:

1. app.config.ts
2. app.component.ts/app.ts
3. folder core > todo-api.service.ts
4. folder feature/todos/components gồm:

   * footer
   * todo-input
   * todo-item
   * components/todos.store.ts

