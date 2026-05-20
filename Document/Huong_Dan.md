# Hướng Dẫn Setup Và Triển Khai LAB 1

## 1. Mục tiêu của tài liệu này

Tài liệu này được tổng hợp từ `Plan.md` và `Setup.md`, với mục tiêu giúp bạn:

- setup đúng bộ khung project ngay từ đầu
- triển khai đúng `100% requirement` bắt buộc của đề
- hoàn thành khoảng `90% các khuyến nghị quan trọng` có ảnh hưởng trực tiếp tới điểm số
- tránh làm lan man vào các phần ngoài phạm vi như JWT, phân quyền, unit test, validation nâng cao

Nếu làm đúng theo thứ tự trong tài liệu này, bạn sẽ có một solution đủ tốt để nộp, demo và bám sát rubric chấm điểm.

## 2. Mục tiêu cần đạt khi hoàn thành bài

Khi kết thúc, project của bạn cần chứng minh được các điểm sau:

- có kiến trúc `3 lớp`: `API`, `Services`, `Repositories`
- có đủ `4 loại model`: `Entity`, `Business`, `Request`, `Response`
- API đúng RESTful
- tất cả API danh sách có `search`, `sort`, `paging`, `fields`, `expand`
- response có format thống nhất
- list API có `pagination metadata`
- `GET by ID` trả dữ liệu liên quan cần thiết
- có đủ dữ liệu mẫu theo đề
- chạy được bằng Docker Compose
- có Swagger để test và mô tả API

## 3. Chiến lược làm bài để đạt điểm cao

Không nên code theo kiểu làm từng CRUD rời rạc. Nên đi theo chiến lược này:

1. Dựng đúng kiến trúc trước.
2. Thiết kế data model và quan hệ hợp lý.
3. Chuẩn hóa model và response wrapper.
4. Hoàn thiện `GET list` và `GET by ID` thật tốt.
5. Bổ sung CRUD còn lại.
6. Hoàn thiện Swagger và Docker.

Lý do là đề chấm mạnh vào kiến trúc, query list API, response format, Swagger và Docker hơn là logic nghiệp vụ phức tạp.

## 4. Cấu trúc solution nên dùng

Tên solution và project nên là:

```text
PRN232.LMS.sln
PRN232.LMS.API
PRN232.LMS.Services
PRN232.LMS.Repositories
```

### Vai trò từng project

- `PRN232.LMS.API`: Controller, route, DI, Swagger, startup, Docker
- `PRN232.LMS.Services`: business logic, orchestration, mapping
- `PRN232.LMS.Repositories`: EF Core, DbContext, truy vấn dữ liệu

### Dependency chuẩn

- `API` tham chiếu `Services`
- `Services` tham chiếu `Repositories`
- `Repositories` không tham chiếu ngược lên `Services` hay `API`

### Lưu ý quan trọng về cấu trúc project

Theo đúng đề bài, phần bắt buộc là bạn phải có 3 project chính:

- `PRN232.[ProjectName].API`
- `PRN232.[ProjectName].Services`
- `PRN232.[ProjectName].Repositories`

Vì vậy, nếu bạn đang làm theo cách:

```text
PRN232.LMS.API
PRN232.LMS.Services
PRN232.LMS.Repositories
```

thì vẫn hoàn toàn hợp lệ, miễn là:

- `Request/Response Models` nằm ở `API`
- `Business Models` nằm ở `Services`
- `Entity Models` nằm ở `Repositories`
- controller không chứa business logic
- repository không chứa business logic

## 5. Các lệnh setup ban đầu

### Tạo solution và project

```bash
dotnet new sln -n PRN232.LMS
dotnet new webapi -n PRN232.LMS.API
dotnet new classlib -n PRN232.LMS.Services
dotnet new classlib -n PRN232.LMS.Repositories
```

### Add project vào solution

```bash
dotnet sln add PRN232.LMS.API/PRN232.LMS.API.csproj
dotnet sln add PRN232.LMS.Services/PRN232.LMS.Services.csproj
dotnet sln add PRN232.LMS.Repositories/PRN232.LMS.Repositories.csproj
```

### Add project reference

```bash
dotnet add PRN232.LMS.API/PRN232.LMS.API.csproj reference PRN232.LMS.Services/PRN232.LMS.Services.csproj
dotnet add PRN232.LMS.Services/PRN232.LMS.Services.csproj reference PRN232.LMS.Repositories/PRN232.LMS.Repositories.csproj
```

## 6. Package cần cài

### Trong `PRN232.LMS.API`

```bash
dotnet add PRN232.LMS.API package Swashbuckle.AspNetCore
dotnet add PRN232.LMS.API package Microsoft.AspNetCore.OpenApi
```

### Trong `PRN232.LMS.Repositories`

```bash
dotnet add PRN232.LMS.Repositories package Microsoft.EntityFrameworkCore
dotnet add PRN232.LMS.Repositories package Microsoft.EntityFrameworkCore.SqlServer
dotnet add PRN232.LMS.Repositories package Microsoft.EntityFrameworkCore.Design
dotnet add PRN232.LMS.Repositories package Microsoft.EntityFrameworkCore.Tools
```

### Có thể cài thêm nếu muốn

```bash
dotnet add PRN232.LMS.Services package AutoMapper
dotnet add PRN232.LMS.Services package AutoMapper.Extensions.Microsoft.DependencyInjection
```

Nếu bạn muốn kiểm soát rõ hơn và dễ giải thích khi bảo vệ bài, map bằng tay là đủ.

## 7. Cấu trúc thư mục khuyến nghị

```text
PRN232.LMS.API/
  Controllers/
  Models/
    Requests/
    Responses/
      Common/
  Program.cs
  appsettings.json

PRN232.LMS.Services/
  Interfaces/
  Implements/
  Models/
    BusinessModels/

PRN232.LMS.Repositories/
  Data/
    LMSDbContext.cs
  Entities/
  Interfaces/
  Implements/
  Migrations/
```

### Nguyên tắc áp dụng cho cấu trúc này

- vẫn giữ đúng 3 layer chính theo đề
- `Request/Response` đã tách khỏi repository
- `BusinessModels` đã tách khỏi API
- `Entities` đã tách khỏi API response
- `DbContext` và migration nằm ở repository là hợp lý
- `API` chỉ nên gọi `Services`, không gọi thẳng `Repositories`
- `Services` không nên trả thẳng `Entity` về controller
- `Repositories` không nên dùng `Request` hay `Response` model
- response trả ra luôn nên là DTO riêng trong `API/Models/Responses`

Đây là cấu trúc chuẩn đang áp dụng trong tài liệu này.

## 8. Thiết kế database nên chốt ngay

### Bảng tối thiểu bắt buộc

- `Semester`
- `Course`
- `Subject`
- `Student`
- `Enrollment`

### Quyết định nên áp dụng

Để `Subject` có ý nghĩa thực tế, nên thêm:

- `Course.SubjectId`

### Quan hệ đề xuất

- `Semester` 1-n `Course`
- `Subject` 1-n `Course`
- `Course` 1-n `Enrollment`
- `Student` 1-n `Enrollment`

### Dữ liệu mẫu bắt buộc

- `5` semesters
- `10` subjects
- `20` courses
- `50` students
- `500` enrollments

## 9. Thiết kế 4 loại model

Đây là phần bắt buộc và rất dễ mất điểm nếu làm sai.

### 1. Entity Model

Dùng để map database.

Ví dụ:

- `Semester`
- `Course`
- `Subject`
- `Student`
- `Enrollment`

### 2. Business Model

Dùng trong service layer để xử lý nghiệp vụ.

Ví dụ:

- `StudentBusinessModel`
- `EnrollmentBusinessModel`

Bạn không nhất thiết phải làm business model quá phức tạp, nhưng nên có để chứng minh tách lớp đúng yêu cầu.

### 3. Request Model

Dùng cho input từ client.

Ví dụ:

- `CreateStudentRequest`
- `UpdateStudentRequest`
- `StudentQueryRequest`
- `CreateEnrollmentRequest`

### 4. Response Model

Dùng cho output trả về API.

Ví dụ:

- `StudentResponse`
- `StudentDetailResponse`
- `EnrollmentResponse`
- `EnrollmentDetailResponse`

### Quy tắc không được vi phạm

- không trả `Entity` trực tiếp ra API
- không dùng `Request/Response` model trong `Repository`
- không để controller xử lý mapping và business logic nặng

## 10. Common model nên tạo sớm

Bạn nên tạo sẵn các class sau trong `API/Models/Responses/Common`:

- `ApiResponse<T>`
- `PagedResponse<T>`
- `PaginationMetadata`
- `ErrorDetail` hoặc `List<string>` cho lỗi

### Format response chuẩn

```json
{
  "success": true,
  "message": "Request processed successfully",
  "data": {},
  "errors": null
}
```

### Format pagination

```json
{
  "page": 1,
  "pageSize": 10,
  "totalItems": 100,
  "totalPages": 10
}
```

## 11. Query model dùng chung cho tất cả list API

Vì đề yêu cầu mọi list API đều có đủ 5 khả năng, bạn nên tạo một request base như sau:

- `Search`
- `Sort`
- `Page`
- `Size`
- `Fields`
- `Expand`

Ví dụ tên class:

- `BaseQueryRequest`

Ví dụ query string:

```text
/api/students?search=nguyen&sort=fullName,-dateOfBirth&page=1&size=10&fields=studentId,fullName,email&expand=enrollments
```

## 12. Cách triển khai repository đúng hướng

Repository chỉ nên làm:

- truy vấn dữ liệu
- filter
- sort
- paging
- include relation để phục vụ `expand`
- lấy entity theo id
- insert, update, delete

Repository không nên làm:

- business validation phức tạp
- format response
- xử lý HTTP status code

### Khuyến nghị thực tế

Nên ưu tiên làm repository tốt cho 3 nhóm trước:

- `Students`
- `Courses`
- `Enrollments`

Sau đó áp dụng cùng pattern cho:

- `Semesters`
- `Subjects`

## 13. Cách triển khai service đúng hướng

Service nên phụ trách:

- validate nghiệp vụ cơ bản
- gọi repository
- xử lý `404`, `400`
- map `Entity -> Business -> Response`
- đóng gói `ApiResponse` hoặc `PagedResponse`

### Ví dụ nhiệm vụ của service

- `GetStudentsAsync(query)`
- `GetStudentByIdAsync(id, expand)`
- `CreateStudentAsync(request)`
- `UpdateStudentAsync(id, request)`
- `DeleteStudentAsync(id)`

## 14. Cách triển khai controller đúng hướng

Controller chỉ nên làm:

- nhận request
- kiểm tra `ModelState` cơ bản
- gọi service
- trả `ActionResult`

Controller không nên làm:

- query EF trực tiếp
- tự viết business rule
- tự map entity sang response quá nhiều

### Route chuẩn RESTful

Đúng:

- `/api/students`
- `/api/students/{id}`
- `/api/enrollments`

Sai:

- `/api/getStudents`
- `/api/createEnrollment`

## 15. Danh sách endpoint nên có

Để đạt đủ requirement và dễ demo, nên làm đủ CRUD cho 5 resource.

### Semesters

- `GET /api/semesters`
- `GET /api/semesters/{id}`
- `POST /api/semesters`
- `PUT /api/semesters/{id}`
- `DELETE /api/semesters/{id}`

### Subjects

- `GET /api/subjects`
- `GET /api/subjects/{id}`
- `POST /api/subjects`
- `PUT /api/subjects/{id}`
- `DELETE /api/subjects/{id}`

### Courses

- `GET /api/courses`
- `GET /api/courses/{id}`
- `POST /api/courses`
- `PUT /api/courses/{id}`
- `DELETE /api/courses/{id}`

### Students

- `GET /api/students`
- `GET /api/students/{id}`
- `POST /api/students`
- `PUT /api/students/{id}`
- `DELETE /api/students/{id}`

### Enrollments

- `GET /api/enrollments`
- `GET /api/enrollments/{id}`
- `POST /api/enrollments`
- `PUT /api/enrollments/{id}`
- `DELETE /api/enrollments/{id}`

## 16. Ưu tiên triển khai theo thứ tự

Đây là thứ tự nên làm để vừa nhanh vừa đúng trọng tâm:

### Giai đoạn 1. Dựng khung

- tạo solution
- tạo project
- add reference
- cài package
- bật Swagger

### Giai đoạn 2. Data layer

- tạo entity
- tạo `LmsDbContext`
- cấu hình Fluent API
- thêm `SubjectId` cho `Course`
- tạo migration
- kết nối SQL Server

### Giai đoạn 3. Seed data

- seed semesters
- seed subjects
- seed courses
- seed students
- seed enrollments

### Giai đoạn 4. Model layer

- tạo business models
- tạo request models
- tạo response models
- tạo `ApiResponse`, `PagedResponse`, `PaginationMetadata`

### Giai đoạn 5. Core query features

- làm `search`
- làm `sort`
- làm `paging`
- làm `expand`
- làm `fields`

Đây là phần quan trọng nhất trong toàn bài.

### Giai đoạn 6. API trọng tâm

Hoàn thiện trước:

- `GET list` cho tất cả resource
- `GET by ID` cho tất cả resource

Sau đó làm tiếp:

- `POST`
- `PUT`
- `DELETE`

### Giai đoạn 7. Swagger và Docker

- annotate response type
- annotate status code
- tạo `Dockerfile`
- tạo `docker-compose.yml`
- test `docker compose up`

## 17. Cách làm `GET list` để đúng đề

Mọi list API nên có cùng pattern xử lý:

1. Nhận `BaseQueryRequest`
2. Tạo `IQueryable`
3. Apply `search`
4. Apply `sort`
5. Apply `expand`
6. Đếm tổng số item
7. Apply `paging`
8. Map sang response
9. Apply `fields`
10. Trả `PagedResponse`

### Gợi ý thực dụng

- `search`: tìm theo các field quan trọng như tên, email, mã môn
- `sort`: hỗ trợ cú pháp `field,-field2`
- `expand`: whitelist field hợp lệ như `student`, `course`, `semester`, `subject`, `enrollments`
- `fields`: chỉ cho phép chọn các field có trong response model

## 18. Cách làm `GET by ID` để đúng đề

Mỗi API chi tiết cần:

- trả `404` nếu không tồn tại
- trả dữ liệu liên quan cần thiết
- không trả object lồng nhau vô hạn

### Ví dụ

`GET /api/students/{id}` nên trả:

- thông tin student
- có thể kèm danh sách enrollment rút gọn
- mỗi enrollment chỉ nên chứa các thông tin cần thiết của course

`GET /api/enrollments/{id}` nên trả:

- thông tin enrollment
- thông tin student rút gọn
- thông tin course rút gọn

Không nên trả nguyên entity navigation theo kiểu vòng lặp.

## 19. Cách làm `fields` và `expand` đơn giản nhưng đủ điểm

Đây là 2 phần khó nhất. Để đạt gần 90% khuyến nghị quan trọng, bạn nên làm theo hướng thực dụng:

### Với `expand`

- chỉ hỗ trợ danh sách tên relation cố định
- ví dụ: `student`, `course`, `semester`, `subject`, `enrollments`
- nếu client truyền sai thì bỏ qua hoặc trả `400` tùy cách bạn muốn chuẩn hóa

### Với `fields`

- map response sang object động hoặc dictionary
- chỉ giữ lại các field hợp lệ được yêu cầu
- validate trước để tránh lỗi runtime

Nếu thời gian không còn nhiều, vẫn phải cố hoàn thành `fields` ở mức cơ bản vì đây là requirement nằm trong phần list API.

## 20. Cách seed data hợp lý

Bạn nên seed theo thứ tự:

1. `Semester`
2. `Subject`
3. `Course`
4. `Student`
5. `Enrollment`

### Nguyên tắc seed

- dữ liệu phải đủ số lượng đề yêu cầu
- dữ liệu nên đa dạng để test search và sort
- enrollment nên phân bố nhiều trạng thái như `Active`, `Completed`, `Dropped`

## 21. Swagger cần làm gì để đủ điểm

Swagger không chỉ bật lên là xong. Bạn nên đảm bảo:

- hiển thị đầy đủ endpoint
- hiển thị request body model
- hiển thị response model
- có mô tả status code `200`, `201`, `400`, `404`, `500`

### Nên làm thêm

- thêm XML comments nếu có thời gian
- dùng `[ProducesResponseType]` cho controller actions

## 22. Docker cần làm gì để đủ điểm

Bạn cần có:

- `Dockerfile`
- `docker-compose.yml`

### `docker-compose.yml` nên có

- service `api`
- service `sqlserver`
- port mapping
- environment variables cho connection string và SQL Server

### Cần test thực tế

```bash
docker compose up --build
```

Bạn nên chứng minh được:

- database container chạy
- API container chạy
- Swagger truy cập được

## 23. Những phần không cần ưu tiên

Theo đề, các phần sau không bắt buộc:

- Authentication
- Authorization
- JWT
- Global exception handling nâng cao
- Unit test
- Integration test
- Validation phức tạp

Nếu thời gian ít, không nên đầu tư vào các phần này trước khi xong list API, response wrapper, Swagger và Docker.

## 24. Checklist hoàn thành 100% requirement

- có đủ 5 bảng tối thiểu
- có đủ số lượng seed data tối thiểu
- có kiến trúc 3 lớp rõ ràng
- có đúng tên project theo pattern đề bài
- có đủ 4 loại model
- không trả entity trực tiếp ra API
- repository không dùng request/response model
- route RESTful theo resource
- tất cả list API có `search`, `sort`, `paging`, `fields`, `expand`
- list response có `pagination`
- `GET by ID` trả dữ liệu liên quan hợp lý
- có response wrapper thống nhất
- dùng đúng `200`, `201`, `400`, `404`, `500`
- có Swagger
- có `Dockerfile`
- có `docker-compose.yml`
- API và DB chạy được bằng Docker Compose

## 25. Checklist để đạt khoảng 90% khuyến nghị quan trọng

- thêm `SubjectId` vào `Course`
- tách riêng project `Models`
- làm CRUD đủ cho 5 resource
- ưu tiên `GET list` và `GET by ID` trước
- làm `expand` bằng whitelist
- làm `fields` bằng object shaping đơn giản
- response model cho endpoint detail và list tách riêng nếu cần
- seed data đủ đa dạng để demo
- annotate Swagger rõ request/response/status code

## 26. Lộ trình làm việc ngắn gọn nhất

Nếu bạn muốn một roadmap cực ngắn để bắt đầu, hãy làm đúng thứ tự này:

1. Tạo solution và 3 project.
2. Add reference và cài package.
3. Tạo entities, DbContext, migration.
4. Seed đủ dữ liệu.
5. Tạo `ApiResponse`, `PagedResponse`, `BaseQueryRequest`.
6. Làm repository query cho `Students`, `Courses`, `Enrollments`.
7. Làm service cho `GET list` và `GET by ID`.
8. Viết controller RESTful.
9. Hoàn thiện CRUD còn lại.
10. Cấu hình Swagger.
11. Viết Dockerfile và docker-compose.
12. Test local và test Docker Compose.

## 27. Kết luận

Muốn hoàn thành bài này tốt, bạn không cần làm backend quá phức tạp. Bạn chỉ cần làm rất chắc các phần mà đề chấm trực tiếp: kiến trúc 3 lớp, 4 loại model, list API mạnh, response thống nhất, Swagger và Docker.

Nếu phải ưu tiên, hãy ưu tiên theo thứ tự:

1. kiến trúc và model separation
2. database và seed data
3. `GET list` và `GET by ID`
4. response wrapper và status code
5. Swagger
6. Docker

Làm chắc 6 phần này là bạn đã đi rất gần một bài nộp đủ yêu cầu và có chất lượng tốt.
