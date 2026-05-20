# Plan

## 1. Mục tiêu bài lab

> `Ghi chú đọc nhanh`
>
> - Các đoạn mang nhãn **[Khuyến nghị]** là gợi ý triển khai, không phải requirement bắt buộc từ đề.
> - Các mục không gắn nhãn khuyến nghị trong phần yêu cầu nên được hiểu là phần cần bám sát khi triển khai.

Xây dựng một **ASP.NET Core RESTful API** cho hệ thống **Learning Management System (LMS)** theo **kiến trúc 3 lớp**:

- API Layer (`Controllers`)
- Service Layer (`Business Logic`)
- Repository Layer (`Data Access`)

Mục tiêu chính của bài không chỉ là CRUD, mà là thể hiện đúng:

- phân tầng kiến trúc
- thiết kế RESTful API
- tách biệt model theo đúng vai trò
- hỗ trợ query nâng cao cho API danh sách
- chuẩn hóa response
- chạy được bằng Docker
- có Swagger/OpenAPI

## 2. Yêu cầu bắt buộc từ đề

### 2.1. Cơ sở dữ liệu tối thiểu

Database phải có ít nhất các bảng sau:

#### `Semester`

- `SemesterId int`
- `SemesterName nvarchar(100)`
- `StartDate datetime`
- `EndDate datetime`

#### `Course`

- `CourseId int`
- `CourseName nvarchar(100)`
- `SemesterId int`

#### `Subject`

- `SubjectId int`
- `SubjectCode varchar(20)`
- `SubjectName nvarchar(100)`
- `Credit int`

#### `Student`

- `StudentId int`
- `FullName nvarchar(100)`
- `Email varchar(100)`
- `DateOfBirth datetime`

#### `Enrollment`

- `EnrollmentId int`
- `StudentId int`
- `CourseId int`
- `EnrollDate datetime`
- `Status varchar(20)`

Đề cho phép **thêm bảng nếu cần**.

### 2.2. Dữ liệu mẫu tối thiểu

Phải generate ít nhất:

- `5` semesters
- `50` students
- `10` subjects
- `20` courses
- `500` enrollments

Đây là yêu cầu bắt buộc vì nó ảnh hưởng trực tiếp tới phần test API danh sách, phân trang, lọc, sắp xếp.

### 2.3. Kiến trúc 3 lớp

Phải áp dụng đúng 3 layer:

- `API Layer`: chỉ nhận request, validate mức cơ bản, gọi service, trả response
- `Service Layer`: xử lý business logic, mapping model, orchestration
- `Repository Layer`: truy cập dữ liệu, query database

Ràng buộc từ đề:

- `Controllers must not contain business logic`
- `Repositories must not contain business logic`
- phải có sự tách biệt trách nhiệm rõ ràng

### 2.4. Quy ước đặt tên project

Project phải đặt tên theo dạng:

- `PRN232.[ProjectName].API`
- `PRN232.[ProjectName].Services`
- `PRN232.[ProjectName].Repositories`

**[Khuyến nghị] Lưu ý triển khai:**

- Đề không ghi riêng project cho Entity/Models/Data, nhưng thực tế có thể tách thêm nếu muốn.
- Nếu tách thêm project khác thì vẫn nên giữ 3 project trên là xương sống chính của solution.

### 2.5. Bắt buộc dùng 4 loại model

Hệ thống phải có đủ 4 nhóm model:

- `Entity Model`: ánh xạ database
- `Business Model`: dùng cho xử lý nghiệp vụ
- `Request Model`: dữ liệu client gửi lên
- `Response Model`: dữ liệu API trả về

Ràng buộc từ đề:

- không được trả thẳng `Entity Model` ra API
- không được dùng `Request/Response Model` trong `Repository Layer`

Đây là điểm chấm rất quan trọng vì nó thể hiện khả năng phân lớp và mapping.

## 3. Yêu cầu về thiết kế API

### 3.1. RESTful API

API phải tuân theo RESTful principles:

- dùng endpoint theo tài nguyên
- URL dùng danh từ số nhiều
- tránh đặt endpoint theo kiểu hành động

Ví dụ đúng:

- `/api/students`
- `/api/students/{id}`
- `/api/enrollments/{id}`

Ví dụ sai:

- `/api/getStudents`
- `/api/createEnrollment`

### 3.2. GET resource by ID

Theo đề, API lấy chi tiết theo ID phải:

- trả về **đầy đủ dữ liệu liên quan**
- tránh circular references và infinite recursion
- trả `404 Not Found` nếu không tồn tại resource

Ví dụ:

- `/api/students/1`
- `/api/enrollments/10`

**[Khuyến nghị] Diễn giải triển khai:**

- `GET /api/students/{id}` nên có thể trả student kèm enrollments hoặc dữ liệu liên quan đã được thiết kế trong response.
- `GET /api/enrollments/{id}` nên có thể trả enrollment kèm student/course thay vì chỉ ID trần.
- Không nên trả object entity gốc có navigation lồng nhau vòng tròn kiểu `Student -> Enrollments -> Student`.

### 3.3. GET collection resource (List API)

Tất cả API danh sách phải hỗ trợ:

- `Searching`: lọc theo từ khóa hoặc điều kiện
- `Sorting`: sắp xếp tăng/giảm theo một hoặc nhiều field
- `Paging`: phân trang bằng page number và page size
- `Selection`: cho phép chọn field cần trả về
- `Expansion`: cho phép include resource liên quan

Ví dụ từ đề:

- `GET /students?search=nguyen`
- `GET /students?sort=fullName,-dateOfBirth`
- `GET /students?page=2&size=10`
- `GET /students?fields=studentId,fullName,email`
- `GET /enrollments?expand=student,course`
- `GET /enrollments?search=active&sort=-enrollDate&page=1&size=20&fields=enrollmentId,status&expand=student,course`

### 3.4. Pagination metadata

Response của API danh sách phải có metadata phân trang dạng:

```json
"pagination": {
  "page": 1,
  "pageSize": 10,
  "totalItems": 100,
  "totalPages": 10
}
```

Điểm cần hiểu đúng:

- Không chỉ trả mỗi mảng dữ liệu.
- Phải có thông tin giúp frontend biết đang ở trang nào, tổng bao nhiêu item, bao nhiêu trang.

## 4. Chuẩn response và HTTP status code

Tất cả API phải trả về response format nhất quán.

Ví dụ từ đề:

```json
{
  "success": true,
  "message": "Request processed successfully",
  "data": {},
  "errors": null
}
```

HTTP status codes bắt buộc đề nêu:

- `200` Success
- `201` Created
- `400` Bad Request
- `404` Not Found
- `500` Internal Server Error

**[Khuyến nghị] Diễn giải triển khai:**

- API không nên lúc thì trả raw object, lúc thì trả `{ data: ... }`.
- Phải thống nhất một wrapper cho toàn bộ API.
- Dù đề ghi `500`, nhưng phần `Global Exception Handling` lại nằm ngoài phạm vi, nên có thể xử lý lỗi mức cơ bản trong controller/service mà không cần middleware exception nâng cao.

## 5. Docker deployment

Đề yêu cầu:

- database phải chạy bằng `Docker Desktop`
- API phải chạy trong `Docker container`
- project phải có:
  - `Dockerfile`
  - `docker-compose.yml`

Sinh viên phải chứng minh được:

- API chạy thành công bằng Docker Compose
- Database chạy thành công bằng Docker Compose

Điều này nghĩa là solution không chỉ chạy local bằng Visual Studio, mà phải có cấu hình container hoàn chỉnh.

## 6. Swagger / OpenAPI

Swagger/OpenAPI là bắt buộc.

Swagger phải hỗ trợ:

- liệt kê endpoint
- test API
- tài liệu request/response
- tài liệu HTTP status code

**[Khuyến nghị] Cách triển khai nên hướng tới:**

- không chỉ bật Swagger UI cho có
- nên annotate hoặc cấu hình để endpoint hiển thị rõ request model, response model, mã trạng thái

## 7. Checklist chấm điểm của đề

Đề chấm dựa trên các ý sau:

- đúng kiến trúc 3 lớp
- đúng cách dùng 4 loại model
- thiết kế RESTful API đúng
- có search, filter, sort, paging
- có pagination metadata
- response format nhất quán
- HTTP status code phù hợp
- hoàn thành Docker deployment
- tích hợp Swagger/OpenAPI

Đây gần như là rubric triển khai trực tiếp. Nếu thiếu một ý trong danh sách này thì rất dễ mất điểm.

## 8. Phần ngoài phạm vi

Đề ghi rõ **không bắt buộc**:

- Authentication / Authorization
- JWT Security
- Advanced Validation
- Global Exception Handling
- Unit Testing / Integration Testing

**[Khuyến nghị] Hiểu nhanh để triển khai:**

- Không cần làm đăng nhập, phân quyền.
- Không cần JWT.
- Không bắt buộc FluentValidation hay validation phức tạp.
- Không bắt buộc middleware exception toàn cục.
- Không bắt buộc viết unit test/integration test.

**[Khuyến nghị]** Nếu thời gian gấp, nên ưu tiên đúng rubric hơn là làm thêm ngoài phạm vi.

## 9. Phân tích kỹ các điểm dễ nhầm

### 9.1. `Subject` đang bị tách rời trong schema gốc

Schema đề bài có bảng `Subject`, nhưng bảng `Course` hiện tại chỉ có:

- `CourseId`
- `CourseName`
- `SemesterId`

Không có `SubjectId`.

Điều này dẫn tới một điểm cần quyết định:

- hoặc giữ nguyên đúng schema tối thiểu và dùng `Subject` như bảng độc lập
- hoặc thêm quan hệ hợp lý như `Course.SubjectId`
- hoặc thêm bảng trung gian nếu muốn mô hình hóa rõ hơn

**[Khuyến nghị]**

- Nên thêm quan hệ hợp lý để `Subject` có vai trò thực tế trong hệ thống, vì đề cho phép thêm bảng/cột nếu cần.
- Nếu không thêm, bảng `Subject` rất dễ trở thành bảng tồn tại cho đủ số lượng nhưng không tham gia nghiệp vụ.

### 9.2. “Return complete related data” không có nghĩa là trả mọi thứ vô hạn

Yêu cầu này nên hiểu là:

- trả đủ dữ liệu liên quan phục vụ client
- nhưng phải thông qua `Response Model`
- và phải chặn vòng lặp tham chiếu

**[Khuyến nghị]**

- Dùng response DTO rõ ràng cho từng endpoint chi tiết.
- Chỉ include các resource liên quan cần thiết.

### 9.3. “All list APIs” là requirement bắt buộc

Câu chữ đề bài là:

- tất cả list APIs phải hỗ trợ searching, sorting, paging, selection, expansion

Vì vậy, nếu bám sát đề thì nên hiểu theo nghĩa chặt:

- mọi endpoint danh sách đều phải có đủ 5 khả năng trên

**[Khuyến nghị triển khai]**

- nếu muốn đúng tuyệt đối theo đề, hãy áp dụng đồng nhất cho tất cả collection endpoints
- nếu triển khai theo từng bước, vẫn nên ưu tiên hoàn thiện `students`, `courses`, `enrollments` trước, sau đó mở rộng cho các resource còn lại

### 9.4. Selection và expansion là phần khó nhất

Hai tính năng thường gây khó:

- `fields=...`: chỉ trả một số field được chọn
- `expand=...`: include resource liên quan

**[Khuyến nghị thực dụng]**

- `expand` có thể hỗ trợ whitelist cố định như `student`, `course`, `semester`
- `fields` có thể triển khai theo dictionary/object shaping ở response layer
- cần validate field hợp lệ để tránh lỗi runtime

## 10. Khuyến nghị phạm vi API nên làm

**[Khuyến nghị] Lưu ý quan trọng:**

- phần này là **khuyến nghị triển khai**
- đề bài **không ghi bắt buộc** phải có đầy đủ CRUD cho mọi bảng

**[Khuyến nghị]** Để bài đầy đủ và dễ demo, nên có:

### Nhóm `Semesters`

- `GET /api/semesters`
- `GET /api/semesters/{id}`
- `POST /api/semesters`
- `PUT /api/semesters/{id}`
- `DELETE /api/semesters/{id}`

### Nhóm `Subjects`

- `GET /api/subjects`
- `GET /api/subjects/{id}`
- `POST /api/subjects`
- `PUT /api/subjects/{id}`
- `DELETE /api/subjects/{id}`

### Nhóm `Courses`

- `GET /api/courses`
- `GET /api/courses/{id}`
- `POST /api/courses`
- `PUT /api/courses/{id}`
- `DELETE /api/courses/{id}`

### Nhóm `Students`

- `GET /api/students`
- `GET /api/students/{id}`
- `POST /api/students`
- `PUT /api/students/{id}`
- `DELETE /api/students/{id}`

### Nhóm `Enrollments`

- `GET /api/enrollments`
- `GET /api/enrollments/{id}`
- `POST /api/enrollments`
- `PUT /api/enrollments/{id}`
- `DELETE /api/enrollments/{id}`

**[Khuyến nghị] Lưu ý triển khai:**

- Đề nhấn mạnh mạnh nhất vào `GET by ID` và `GET list`.
- Nếu thời gian hạn chế, nên đảm bảo 2 nhóm này thật tốt trước.

## 11. Đề xuất cấu trúc solution

**[Khuyến nghị]** Một cấu trúc solution hợp lý:

```text
PRN232.LMS.API
PRN232.LMS.Services
PRN232.LMS.Repositories
PRN232.LMS.Models
```

Trong đó:

- `API`: controllers, swagger config, program startup, docker config
- `Services`: business services, interfaces, mapping orchestration
- `Repositories`: DbContext, repositories, EF queries
- `Models`: entities, business models, request models, response models

**[Khuyến nghị]** Nếu muốn bám sát tuyệt đối tên đề:

- bắt buộc nên có đủ `API`, `Services`, `Repositories`
- project `Models` là phần mở rộng hợp lý

## 12. Kế hoạch triển khai đề xuất

> Toàn bộ mục này là **[Khuyến nghị]** để bạn bắt đầu triển khai công việc theo thứ tự hợp lý.

### Giai đoạn 1. Dựng solution và kiến trúc

- tạo solution và 3 project chính theo naming convention
- cấu hình dependency giữa các project
- setup Entity Framework Core
- cấu hình `DbContext`

### Giai đoạn 2. Thiết kế dữ liệu

- tạo entity cho 5 bảng tối thiểu
- bổ sung quan hệ còn thiếu nếu cần, đặc biệt giữa `Course` và `Subject`
- tạo migration
- seed/generate data đủ số lượng đề yêu cầu

### Giai đoạn 3. Thiết kế model

- tách rõ:
  - `Entities`
  - `Business Models`
  - `Request Models`
  - `Response Models`
- thiết lập mapping giữa các lớp model

### Giai đoạn 4. Repository layer

- viết repository interfaces
- viết repository implementations
- tập trung vào query cho list API:
  - search
  - sort
  - paging
  - expand

### Giai đoạn 5. Service layer

- xử lý nghiệp vụ CRUD
- xử lý mapping entity -> business -> response
- kiểm soát các case `404`, `400`
- đóng gói response dữ liệu danh sách có pagination metadata

### Giai đoạn 6. API layer

- viết controllers RESTful
- chuẩn hóa route dạng `/api/[resources]`
- dùng response wrapper thống nhất
- cấu hình status code đúng từng case

### Giai đoạn 7. Swagger

- bật Swagger/OpenAPI
- đảm bảo request/response model hiển thị rõ
- mô tả status code cho endpoint chính

### Giai đoạn 8. Docker

- viết `Dockerfile`
- viết `docker-compose.yml`
- cấu hình API container + DB container
- test chạy bằng `docker compose up`

## 13. Checklist nghiệm thu trước khi nộp

- Solution có đúng naming convention chưa
- Controller có chứa business logic không
- Repository có chứa business logic không
- Có đủ 4 loại model chưa
- API có trả entity trực tiếp không
- Repository có dùng request/response model không
- URL có là plural nouns và resource-based không
- `GET by ID` có trả `404` khi không tìm thấy không
- `GET list` có search/sort/paging/fields/expand không
- Tất cả API danh sách có cùng hỗ trợ `search`, `sort`, `paging`, `selection`, `expansion` không
- Response list có `pagination` không
- Response toàn hệ thống có cùng format không
- Có dùng đúng `200/201/400/404/500` không
- Có `Dockerfile` và `docker-compose.yml` không
- API và DB có chạy được bằng Docker Compose không
- Swagger có hiển thị endpoint, request/response, status code không

## 14. Kết luận ngắn

Đề này tập trung vào **thiết kế backend đúng chuẩn** hơn là làm tính năng phức tạp. Các điểm quyết định điểm số cao nhất là:

- phân tầng đúng
- model tách đúng vai trò
- list API đủ khả năng query
- response thống nhất
- Docker + Swagger hoàn chỉnh

**[Khuyến nghị]** Nếu cần ưu tiên, nên làm theo thứ tự:

1. kiến trúc 3 lớp
2. entity + seed data
3. request/response/business model
4. `GET list` và `GET by ID`
5. response wrapper + status code
6. Swagger
7. Docker

## 15. Ghi chú nguồn phân tích

File này được phân tích từ:

- nội dung PDF gốc `PRN232- LAB 1 - Rest API Basics and Deployment.pdf`
- phần text bạn đã copy trong yêu cầu

Nội dung ở các mục `4` đến `10` đã được đối chiếu trực tiếp từ PDF.
