# LAB1 Checklist & Implementation Plan

Tài liệu này đối chiếu trực tiếp với `Huong_Dan.md` trong thư mục `Document/`, đồng thời phản ánh hiện trạng đã kiểm tra từ source code trong `Src/`.

Mục tiêu của file này:

1. Ghi lại checklist đánh giá hiện trạng project theo từng requirement với trạng thái `Đã có` / `Thiếu` / `Ưu tiên làm tiếp`.
2. Chuyển checklist đó thành lộ trình thực hiện theo thứ tự an toàn để đạt `100% requirement bắt buộc` trước, sau đó mới tối ưu để tăng điểm.

Ưu tiên hiện tại là `nộp bài chắc`: hoàn thành đầy đủ các phần bị chấm trực tiếp trước, chưa mở rộng sang các phần ngoài phạm vi như `Authentication`, `Authorization`, `JWT`, `Unit Test`, `Integration Test`, hoặc validation nâng cao.

## 1. Checklist Hiện Trạng

| Nhóm requirement | Hạng mục | Trạng thái | Bằng chứng hiện tại | Việc cần làm tiếp | Ưu tiên |
| --- | --- | --- | --- | --- | --- |
| Kiến trúc 3 lớp | Có `API`, `Services`, `Repositories` và dependency đúng chiều | Đã có | `Src/PRN232.LMS.API`, `Src/PRN232.LMS.Services`, `Src/PRN232.LMS.Repositories`; `API` reference `Services`, `Services` reference `Repositories` | Giữ nguyên kiến trúc hiện tại, phát triển tiếp theo đúng layering | Cao |
| Tên solution/project | Project name đúng pattern, solution đang là `.slnx` | Gần đạt | Có `PRN232.LMS.API`, `PRN232.LMS.Services`, `PRN232.LMS.Repositories`, `PRN232.LMS.slnx` | Cân nhắc đổi sang `.sln` nếu cần bám sát tuyệt đối theo guide | Trung bình |
| Packages bắt buộc | Swagger/OpenAPI trong `API`, EF Core trong `Repositories` | Đã có | `Swashbuckle.AspNetCore`, `Microsoft.AspNetCore.OpenApi`, `Microsoft.EntityFrameworkCore`, `SqlServer`, `Design`, `Tools` đã được cài | Không cần bổ sung thêm package bắt buộc ở giai đoạn này | Trung bình |
| Cấu trúc thư mục khuyến nghị | Có các thư mục chính theo 3 layer | Đã có một phần | Có `Controllers`, `Models`, `BusinessModels`, `Entities`, `Data`, `Interfaces`, `Implements`, `Migrations` | Bổ sung các controller/service/resource-specific classes còn thiếu | Cao |
| 5 entity tối thiểu | `Semester`, `Subject`, `Course`, `Student`, `Enrollment` | Đã có | Đã có đủ entity trong `PRN232.LMS.Repositories/Entities` | Giữ nguyên, chỉ mở rộng nếu cần cho detail response | Cao |
| Quan hệ DB + `SubjectId` cho `Course` | Quan hệ chính đã được cấu hình | Đã có | `LmsDbContext` có khóa ngoại `Course -> Semester`, `Course -> Subject`, `Enrollment -> Student`, `Enrollment -> Course`; `Course` có `SubjectId` | Không cần đổi mô hình data ở vòng đầu | Cao |
| Migration | Có migration ban đầu | Đã có | Có thư mục `Migrations` với `InitialCreate` | Tạo migration mới chỉ khi có thay đổi schema thật sự | Trung bình |
| Seed data tối thiểu | Đủ số lượng seed yêu cầu | Đã có | Có seed `5` semesters, `10` subjects, `20` courses, `50` students, `500` enrollments | Giữ seed ổn định để test query/list/detail | Cao |
| 4 loại model | Entity / Business / Request / Response đã tồn tại | Đã có ở mức khai báo | Có `Entities`, `BusinessModels`, `Models/Requests`, `Models/Responses` | Kết nối đầy đủ các model này vào luồng CRUD thực tế | Cao |
| Response wrapper chuẩn | Có `ApiResponse`, `PagedResponse`, `PaginationMetadata` | Đã có một phần | Đã có class common response trong `API/Models/Responses/Common` | Dùng thống nhất cho toàn bộ resource APIs | Cao |
| Base query model | Có `Search`, `Sort`, `Page`, `Size`, `Fields`, `Expand` | Đã có | Đã có `BaseQueryRequest` | Áp dụng thực tế cho toàn bộ list APIs | Cao |
| CRUD cho 5 resource | Chưa có các API nghiệp vụ chính | Thiếu | Hiện chỉ có `HealthController` và `SystemController` | Tạo đủ CRUD cho `Semesters`, `Subjects`, `Courses`, `Students`, `Enrollments` | Rất cao |
| RESTful routes | Chưa có route theo resource | Thiếu | Chưa có `/api/semesters`, `/api/subjects`, `/api/courses`, `/api/students`, `/api/enrollments` | Tạo route chuẩn RESTful cho 5 resource | Rất cao |
| `GET list` có `search/sort/paging/fields/expand` | Chưa áp dụng thực tế | Thiếu | `BaseQueryRequest` mới tồn tại ở mức model | Triển khai đầy đủ cho tất cả list APIs | Rất cao |
| `GET by ID` có related data hợp lý | Chưa có | Thiếu | Chưa có detail endpoints cho 5 resource | Tạo response detail và include dữ liệu liên quan cần demo | Rất cao |
| Không trả entity trực tiếp | Chưa xác minh đầy đủ | Chưa xác minh đầy đủ | Chưa có resource API để kiểm tra end-to-end | Bắt buộc trả `Response Model` thay vì `Entity` khi triển khai CRUD | Cao |
| Swagger mô tả request/response/status code | Mới cấu hình cơ bản | Đã có một phần | Swagger đã bật, nhưng controller hiện tại mới annotate `200 OK` cơ bản | Thêm `[ProducesResponseType]` cho CRUD, mô tả `200/201/400/404/500` | Cao |
| Dockerfile | Có Dockerfile cho API | Đã có | Đã có `Src/PRN232.LMS.API/Dockerfile` | Chỉ rà lại nếu phát sinh lỗi runtime khi compose | Trung bình |
| `docker-compose.yml` | Có compose file | Đã có | Đã có `Src/docker-compose.yml` cho `api` và `sqlserver` | Rà lại env vars và test runtime | Trung bình |
| Chạy được bằng Docker Compose | Chưa xác minh thực tế | Chưa xác minh | Môi trường kiểm tra hiện tại chưa có Docker CLI | Test `docker compose up --build` trên máy có Docker | Cao |

## 2. Checklist Triển Khai Theo Giai Đoạn

### Giai đoạn 1: Hoàn thiện khung API bắt buộc

- [ ] Tạo controller cho `Semesters`, `Subjects`, `Courses`, `Students`, `Enrollments`.
- [ ] Tạo service interface và service implementation tương ứng cho 5 resource.
- [ ] Tiếp tục dùng `generic repository` làm nền, nhưng bổ sung query support hoặc repository chuyên biệt nếu cần để xử lý list/detail tốt hơn.
- [ ] Chuẩn hóa `DI registration` cho toàn bộ resource layer.

### Giai đoạn 2: Hoàn thiện list APIs

- [ ] Áp dụng `BaseQueryRequest` cho tất cả endpoint `GET list`.
- [ ] Hỗ trợ `search`.
- [ ] Hỗ trợ `sort` theo cú pháp `field,-field2`.
- [ ] Hỗ trợ `paging`.
- [ ] Hỗ trợ `expand` bằng whitelist relation hợp lệ cho từng resource.
- [ ] Hỗ trợ `fields` bằng object shaping an toàn trên response model.
- [ ] Trả `PagedResponse<T>` có `Pagination`.

### Giai đoạn 3: Hoàn thiện detail APIs

- [ ] Tạo `GET /api/{resource}/{id}` cho cả 5 resource.
- [ ] Trả `404` khi id không tồn tại.
- [ ] Tạo response detail riêng nếu cần để chứa related data.
- [ ] Không trả navigation lồng nhau vô hạn.
- [ ] Chỉ include dữ liệu liên quan thật sự cần để demo.

### Giai đoạn 4: Hoàn thiện CRUD còn lại

- [ ] Tạo `POST` cho 5 resource.
- [ ] Tạo `PUT` cho 5 resource.
- [ ] Tạo `DELETE` cho 5 resource.
- [ ] Validate cơ bản ở service/controller.
- [ ] Trả đúng `200`, `201`, `400`, `404`.

### Giai đoạn 5: Swagger và hoàn thiện nộp bài

- [ ] Thêm `[ProducesResponseType]` cho các action CRUD chính.
- [ ] Đảm bảo Swagger hiển thị đầy đủ request body và response model.
- [ ] Giữ `Health` và `System/readiness` như endpoint hỗ trợ demo.
- [ ] Rà soát lại việc dùng response wrapper để đảm bảo thống nhất giữa các resource.

### Giai đoạn 6: Docker và xác minh cuối

- [ ] Kiểm tra lại connection string local và compose.
- [ ] Chạy `docker compose up --build`.
- [ ] Xác minh `SQL Server container` khởi động thành công.
- [ ] Xác minh `API container` khởi động thành công.
- [ ] Xác minh Swagger truy cập được qua cổng đã map.

## 3. Thay Đổi Kỹ Thuật Cần Thực Hiện Trong Code

### Public API / Route Contract

- Thêm đủ `25 endpoint CRUD` cho 5 resource theo chuẩn RESTful:
  - `/api/semesters`
  - `/api/subjects`
  - `/api/courses`
  - `/api/students`
  - `/api/enrollments`
- Toàn bộ list endpoint cần nhận query chuẩn:
  - `search`
  - `sort`
  - `page`
  - `size`
  - `fields`
  - `expand`

### Service / Repository / Model Contract

- Mỗi resource cần service riêng với tối thiểu các hàm:
  - `GetListAsync(query)`
  - `GetByIdAsync(id, expand)`
  - `CreateAsync(request)`
  - `UpdateAsync(id, request)`
  - `DeleteAsync(id)`
- Tạo hoặc mở rộng response model cho list/detail riêng nếu cần.
- Tạo whitelist cho `expand` và `fields` theo từng resource.
- Repository layer không dùng `Request/Response model` và không format `HTTP response`.

## 4. Test Plan

- [ ] `dotnet build` solution phải pass.
- [ ] Swagger phải hiện đủ endpoint cho 5 resource.
- [ ] Mỗi resource phải test được:
  - [ ] `GET list` trả `PagedResponse`.
  - [ ] `GET list` với `search`.
  - [ ] `GET list` với `sort`.
  - [ ] `GET list` với `page/size`.
  - [ ] `GET list` với `fields`.
  - [ ] `GET list` với `expand`.
  - [ ] `GET by ID` trả related data hợp lý.
  - [ ] `GET by ID` với sai id trả `404`.
  - [ ] `POST` hợp lệ trả `201`.
  - [ ] `PUT` hợp lệ trả `200`.
  - [ ] `DELETE` hợp lệ trả `200` hoặc `204`, nhưng phải thống nhất toàn project.
- [ ] Endpoint `System/readiness` phải báo đủ số lượng seed tối thiểu.
- [ ] Docker Compose phải khởi động được cả DB và API.

## 5. Assumptions Và Default Đã Chốt

- File này được đặt tại `Document/LAB1_Checklist_Plan.md`.
- Nội dung được viết bằng tiếng Việt để thuận tiện bám bài nộp và demo.
- Ưu tiên hoàn thành `100% requirement bắt buộc` trước phần tăng điểm.
- Giữ kiến trúc `3 project` hiện tại, chưa tách thêm project `Models` ở vòng đầu.
- Tiếp tục dùng `generic repository` nếu còn phù hợp; chỉ bổ sung repository chuyên biệt khi thật sự cần cho `list/detail APIs`.
- `Health` và `System/readiness` chỉ là endpoint hỗ trợ, không thay thế các resource API bắt buộc.
