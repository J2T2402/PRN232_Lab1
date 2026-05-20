# Setup

## 1. Mục tiêu giai đoạn setup

Giai đoạn setup cần tạo ra một bộ khung chạy được cho bài lab LMS, đúng rubric ngay từ đầu để tránh phải sửa kiến trúc về sau. Sau khi hoàn thành setup, project nên có:

- solution theo đúng naming convention
- cấu trúc 3 lớp rõ ràng
- project chứa model tách riêng
- kết nối database bằng EF Core
- seed data đủ để phát triển và test list API
- Swagger chạy được
- Dockerfile và docker-compose khởi tạo được API + DB

## 2. Quyết định kiến trúc ban đầu

Để bám sát đề và vẫn dễ mở rộng, nên chốt solution theo cấu trúc:

```text
PRN232.LMS.API
PRN232.LMS.Services
PRN232.LMS.Repositories
PRN232.LMS.Models
PRN232.LMS.sln
```

### Vai trò từng project

- `PRN232.LMS.API`: Controllers, cấu hình DI, Swagger, middleware cơ bản, startup, Docker
- `PRN232.LMS.Services`: business logic, service interfaces, mapping orchestration
- `PRN232.LMS.Repositories`: DbContext, entity configuration, repositories, truy vấn dữ liệu
- `PRN232.LMS.Models`: Entities, Business Models, Request Models, Response Models, dùng chung constants/enums nếu cần

## 3. Quy ước dependency

Để tránh lẫn trách nhiệm giữa các layer, nên setup reference như sau:

- `API` tham chiếu `Services` và `Models`
- `Services` tham chiếu `Repositories` và `Models`
- `Repositories` tham chiếu `Models`
- `Models` không tham chiếu project nào khác

Không để `API` gọi trực tiếp `Repositories`.

## 4. Thiết kế dữ liệu cần chốt sớm

Theo phân tích trong `Plan.md`, điểm dễ vướng nhất là bảng `Subject` đang rời rạc. Nên chốt ngay từ đầu:

- thêm `SubjectId` vào `Course`
- `Semester` 1-n `Course`
- `Subject` 1-n `Course`
- `Student` 1-n `Enrollment`
- `Course` 1-n `Enrollment`

### Entity tối thiểu

- `Semester`
- `Subject`
- `Course`
- `Student`
- `Enrollment`

### Seed data tối thiểu

- `5` semesters
- `10` subjects
- `20` courses
- `50` students
- `500` enrollments

## 5. Cấu trúc model nên dựng từ đầu

Trong `PRN232.LMS.Models`, nên chia thư mục rõ ngay khi setup:

```text
Entities/
Business/
Requests/
Responses/
Common/
```

### Gợi ý chi tiết

- `Entities/`: class map database
- `Business/`: model phục vụ service layer
- `Requests/`: model cho create/update/filter query
- `Responses/`: model trả về client
- `Common/`: `ApiResponse<T>`, `PagedResult<T>`, `PaginationMetadata`, enum trạng thái

## 6. Kế hoạch setup theo thứ tự thực hiện

### Bước 1. Dựng solution và project

- tạo `PRN232.LMS.sln`
- tạo 4 project theo cấu trúc trên
- add project vào solution
- add project reference đúng dependency

### Bước 2. Cài package nền

#### `PRN232.LMS.API`

- `Swashbuckle.AspNetCore`
- `Microsoft.AspNetCore.OpenApi`

#### `PRN232.LMS.Repositories`

- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.Design`
- `Microsoft.EntityFrameworkCore.Tools`

#### Có thể thêm nếu nhóm muốn mapping gọn hơn

- `AutoMapper`
- `AutoMapper.Extensions.Microsoft.DependencyInjection`

Nếu thời gian gấp, có thể map bằng tay để dễ kiểm soát và bám sát yêu cầu.

### Bước 3. Tạo folder skeleton cho từng project

#### `PRN232.LMS.API`

- `Controllers/`
- `Configurations/`
- `Extensions/`

#### `PRN232.LMS.Services`

- `Interfaces/`
- `Implementations/`
- `Mappings/`

#### `PRN232.LMS.Repositories`

- `Data/`
- `Interfaces/`
- `Implementations/`
- `Seeders/`

#### `PRN232.LMS.Models`

- `Entities/`
- `Business/`
- `Requests/`
- `Responses/`
- `Common/`

### Bước 4. Setup EF Core và database

- tạo `LmsDbContext`
- khai báo `DbSet` cho 5 entity
- cấu hình relation bằng Fluent API
- cấu hình seed strategy
- tạo migration đầu tiên

### Bước 5. Chuẩn bị seed data

Nên tách seed thành logic riêng thay vì hard-code trong controller:

- `SemesterSeeder`
- `SubjectSeeder`
- `CourseSeeder`
- `StudentSeeder`
- `EnrollmentSeeder`

Mục tiêu của bước này là khi chạy app hoặc migration xong, dữ liệu mẫu đã đủ để test:

- search
- sort
- paging
- fields
- expand

### Bước 6. Chuẩn hóa response từ đầu

Trước khi viết controller, nên tạo sẵn:

- `ApiResponse<T>`
- `PagedResponse<T>`
- `PaginationMetadata`

Format mục tiêu:

```json
{
  "success": true,
  "message": "Request processed successfully",
  "data": {},
  "errors": null
}
```

List API nên trả thêm:

```json
"pagination": {
  "page": 1,
  "pageSize": 10,
  "totalItems": 100,
  "totalPages": 10
}
```

### Bước 7. Chuẩn hóa query model cho list API

Vì tất cả list API đều phải có search, sort, paging, selection, expansion, nên nên dựng sẵn một query model dùng chung, ví dụ:

- `Search`
- `Sort`
- `Page`
- `Size`
- `Fields`
- `Expand`

Có thể đặt trong `Requests/Common/BaseQueryRequest.cs`.

### Bước 8. Setup Swagger

- bật Swagger trong `Program.cs`
- cấu hình hiển thị XML comments nếu có
- chuẩn bị annotate status code cho controller về sau

Mục tiêu ở giai đoạn setup là API project chạy lên phải thấy Swagger UI ngay.

### Bước 9. Setup Docker

Chuẩn bị ngay từ đầu để tránh lệch cấu hình local:

- `Dockerfile` cho `PRN232.LMS.API`
- `docker-compose.yml` gồm:
  - `api`
  - `sqlserver`

Nên dùng biến môi trường cho:

- `ConnectionStrings__DefaultConnection`
- `SA_PASSWORD`
- `ACCEPT_EULA`

## 7. Thứ tự ưu tiên implement sau setup

Sau khi setup xong, nên triển khai theo thứ tự này:

1. Entity + DbContext + migration
2. Seed data
3. Common response models
4. Repository cho `Student`, `Course`, `Enrollment`
5. Service cho `GET list` và `GET by id`
6. Controller + Swagger
7. CRUD còn lại
8. Docker hoàn chỉnh và test compose

Lý do là rubric chấm nặng nhất ở kiến trúc, model separation, list API, response format, Docker, Swagger.

## 8. Định nghĩa hoàn thành cho phase setup

Có thể xem setup hoàn tất khi đạt đủ các điều kiện sau:

- solution build thành công
- project references đúng 3-layer architecture
- DbContext kết nối được SQL Server
- migration đầu tiên chạy được
- seed được dữ liệu mẫu
- Swagger mở được
- `docker compose up` dựng được API và database
- chưa cần hoàn chỉnh toàn bộ CRUD, nhưng bộ khung phải sẵn sàng để phát triển

## 9. Checklist bắt đầu làm việc

- chốt tên project là `LMS`
- tạo solution và 4 project
- thêm project references
- cài package EF Core và Swagger
- dựng `LmsDbContext`
- tạo 5 entities và relation
- thêm `SubjectId` cho `Course`
- tạo migration đầu tiên
- chuẩn bị seed data
- tạo response wrapper và pagination models
- bật Swagger
- tạo Dockerfile và docker-compose

## 10. Gợi ý command để bắt đầu

```bash
dotnet new sln -n PRN232.LMS
dotnet new webapi -n PRN232.LMS.API
dotnet new classlib -n PRN232.LMS.Services
dotnet new classlib -n PRN232.LMS.Repositories
dotnet new classlib -n PRN232.LMS.Models
```

```bash
dotnet sln add PRN232.LMS.API/PRN232.LMS.API.csproj
dotnet sln add PRN232.LMS.Services/PRN232.LMS.Services.csproj
dotnet sln add PRN232.LMS.Repositories/PRN232.LMS.Repositories.csproj
dotnet sln add PRN232.LMS.Models/PRN232.LMS.Models.csproj
```

```bash
dotnet add PRN232.LMS.API/PRN232.LMS.API.csproj reference PRN232.LMS.Services/PRN232.LMS.Services.csproj
dotnet add PRN232.LMS.API/PRN232.LMS.API.csproj reference PRN232.LMS.Models/PRN232.LMS.Models.csproj
dotnet add PRN232.LMS.Services/PRN232.LMS.Services.csproj reference PRN232.LMS.Repositories/PRN232.LMS.Repositories.csproj
dotnet add PRN232.LMS.Services/PRN232.LMS.Services.csproj reference PRN232.LMS.Models/PRN232.LMS.Models.csproj
dotnet add PRN232.LMS.Repositories/PRN232.LMS.Repositories.csproj reference PRN232.LMS.Models/PRN232.LMS.Models.csproj
```

## 11. Kết luận ngắn

Setup của bài này không chỉ là tạo project, mà là chốt đúng khung kiến trúc để các phần sau như list API, response wrapper, Swagger và Docker không bị làm lại. Ưu tiên cao nhất là dựng đúng solution, đúng model separation, đúng database design và chuẩn bị seed data đủ lớn ngay từ đầu.
