# Hướng Dẫn Chụp Evidence Theo Checklist

File này giúp bạn chuẩn bị ảnh chụp màn hình và bằng chứng để chứng minh bài làm đã đáp ứng những mục nào trong checklist ở [CheckList.md](./CheckList.md).

## 1. Mục tiêu

Sau khi làm theo file này, bạn sẽ có đủ evidence để chứng minh:

- Kiến trúc `3-layer`
- Tên project đúng convention
- Database schema và seed data đúng yêu cầu
- Có đủ `Entity / Business / Request / Response model`
- API đúng chuẩn RESTful
- `GET by ID` có related data, có `404`
- `GET list` có `search / sort / paging / fields / expand`
- Response wrapper và pagination metadata đúng chuẩn
- Có Swagger/OpenAPI
- Có Dockerfile và `docker-compose.yml`
- Project build thành công

## 2. Bộ ảnh nên chuẩn bị

Bạn nên tạo thư mục lưu ảnh ví dụ:

```text
Document/Evidence/
```

Tên file ảnh gợi ý:

- `01-solution-structure.png`
- `02-project-naming.png`
- `03-build-success.png`
- `04-db-schema.png`
- `05-seed-counts.png`
- `06-swagger-overview.png`
- `07-semesters-list.png`
- `08-semesters-detail-expand.png`
- `09-students-search-sort-page.png`
- `10-fields-selection.png`
- `11-404-not-found.png`
- `12-create-update-delete.png`
- `13-docker-files.png`
- `14-docker-compose-config.png`
- `15-docker-up.png`

## 3. Chuẩn bị trước khi chụp

## Cách chạy local

Mở terminal tại `d:\PRN232\LAB1\Src` rồi chạy:

```powershell
dotnet build PRN232.LMS.sln
dotnet run --project PRN232.LMS.API
```

URL local theo `launchSettings.json`:

- `http://localhost:5125/swagger`
- `https://localhost:7043/swagger`

Nếu máy hỏi chứng chỉ HTTPS, bạn có thể dùng URL HTTP để chụp evidence nhanh hơn.

## Nếu muốn chạy bằng Docker

Chạy tại `d:\PRN232\LAB1`:

```powershell
docker compose -f Src\docker-compose.yml config
docker compose -f Src\docker-compose.yml up -d --build
docker compose -f Src\docker-compose.yml ps
```

Swagger của container:

- `http://localhost:8080/swagger`

## 4. Evidence theo từng mục checklist

## 4.1. 3-layer Architecture - 1.2 điểm

### Cần chụp

- Cây thư mục solution hiển thị 3 project:
  - `PRN232.LMS.API`
  - `PRN232.LMS.Services`
  - `PRN232.LMS.Repositories`
- Nếu được, mở thêm `Program.cs` và `ServiceCollectionExtensions.cs` để thấy dependency đi đúng chiều.

### Cách chụp đẹp

- Mở Visual Studio hoặc VS Code Explorer.
- Chụp ảnh toàn bộ solution.
- Chụp thêm code DI nếu cần.

### Bằng chứng code

- `Src/PRN232.LMS.API/Program.cs`
- `Src/PRN232.LMS.Services/ServiceCollectionExtensions.cs`

## 4.2. Project Naming Convention - 0.4 điểm

### Cần chụp

- Tên solution
- Tên 3 project đúng format:
  - `PRN232.LMS.API`
  - `PRN232.LMS.Services`
  - `PRN232.LMS.Repositories`

### Ảnh gợi ý

- Chụp ngay trong Solution Explorer cùng ảnh mục `3-layer`.

## 4.3. DB Schema & Seed Data - 0.8 điểm

### Cần chụp

- DB có đủ bảng:
  - `Semesters`
  - `Subjects`
  - `Courses`
  - `Students`
  - `Enrollments`
- Số lượng seed:
  - `5` semesters
  - `10` subjects
  - `20` courses
  - `50` students
  - `500` enrollments

### Cách lấy evidence

Mở SQL Server và chạy:

```sql
SELECT COUNT(*) AS SemesterCount FROM Semesters;
SELECT COUNT(*) AS SubjectCount FROM Subjects;
SELECT COUNT(*) AS CourseCount FROM Courses;
SELECT COUNT(*) AS StudentCount FROM Students;
SELECT COUNT(*) AS EnrollmentCount FROM Enrollments;
```

### Ảnh nên chụp

- 1 ảnh danh sách bảng
- 1 ảnh kết quả query đếm dữ liệu

### Nếu muốn chụp thêm schema quan hệ

- Mở Database Diagram hoặc Object Explorer.
- Chụp quan hệ:
  - `Course -> Semester`
  - `Course -> Subject`
  - `Enrollment -> Student`
  - `Enrollment -> Course`

## 4.4. 4 Model Types - 1.0 điểm

### Cần chụp

- Có thư mục và file cho:
  - `Entities`
  - `BusinessModels`
  - `Models/Requests`
  - `Models/Responses`

### Ảnh gợi ý

- Chụp explorer hiển thị:
  - `Src/PRN232.LMS.Repositories/Entities`
  - `Src/PRN232.LMS.Services/Models/BusinessModels`
  - `Src/PRN232.LMS.API/Models/Requests`
  - `Src/PRN232.LMS.API/Models/Responses`

### Gợi ý nói khi demo

- Repository dùng `Entity`
- Service dùng `BusinessModel`
- API nhận `Request`, trả `Response`
- Không trả `Entity` trực tiếp ra ngoài

## 4.5. RESTful Endpoint Naming - 0.7 điểm

### Cần chụp

- Swagger hiển thị route theo resource-based, plural nouns:
  - `/api/semesters`
  - `/api/subjects`
  - `/api/courses`
  - `/api/students`
  - `/api/enrollments`

### Ảnh gợi ý

- Chụp màn hình Swagger overview, thấy đủ 5 nhóm endpoint.

## 4.6. GET by ID - 0.6 điểm

### Mục tiêu cần chứng minh

- Lấy chi tiết theo ID
- Có related data khi cần
- Không bị circular reference
- Sai ID trả `404`

### Test gợi ý

#### Semester detail có expand/default related data

```text
GET http://localhost:5125/api/semesters/1
```

Hoặc:

```text
GET http://localhost:5125/api/semesters/1?expand=courses
```

#### Student detail

```text
GET http://localhost:5125/api/students/1?expand=enrollments
```

#### Course detail

```text
GET http://localhost:5125/api/courses/1?expand=semester,subject,enrollments
```

#### Not found

```text
GET http://localhost:5125/api/subjects/999999
```

### Ảnh nên chụp

- 1 ảnh `GET by ID` thành công có related data
- 1 ảnh `404 Not Found`

## 4.7. List API Capabilities - 1.5 điểm

Bạn nên chụp ít nhất 4 ảnh để chứng minh đủ các khả năng.

### A. Search

```text
GET http://localhost:5125/api/students?search=student001
```

Hoặc:

```text
GET http://localhost:5125/api/subjects?search=SUB001
```

### B. Sort

```text
GET http://localhost:5125/api/students?sort=fullname
GET http://localhost:5125/api/students?sort=-fullname
GET http://localhost:5125/api/courses?sort=semestername,-coursename
```

### C. Paging

```text
GET http://localhost:5125/api/enrollments?page=2&size=5
```

### D. Fields selection

```text
GET http://localhost:5125/api/subjects?fields=subjectId,subjectCode,subjectName
```

### E. Expand

```text
GET http://localhost:5125/api/courses?expand=semester,subject,enrollments
GET http://localhost:5125/api/enrollments?expand=student,course
```

### Ảnh nên chụp

- `search`
- `sort`
- `paging`
- `fields`
- `expand`

Nếu muốn gọn hơn, có thể chụp 2 ảnh nhưng mỗi ảnh phải thể hiện rõ nhiều query parameters và response tương ứng.

## 4.8. Pagination Metadata - 0.4 điểm

### Cần chụp

Một response list có các field:

- `page`
- `pageSize`
- `totalItems`
- `totalPages`

### API gợi ý

```text
GET http://localhost:5125/api/enrollments?page=1&size=10
```

### Ảnh nên chụp

- Chụp phần `pagination` trong response JSON.

## 4.9. Response Format & HTTP Status - 0.8 điểm

### Cần chụp

Response theo format thống nhất:

```json
{
  "success": true,
  "message": "...",
  "data": ...,
  "errors": null
}
```

Và có các status:

- `200 OK`
- `201 Created`
- `400 Bad Request`
- `404 Not Found`

### Test gợi ý

#### 200 OK

```text
GET http://localhost:5125/api/semesters
```

#### 201 Created

```http
POST http://localhost:5125/api/subjects
Content-Type: application/json

{
  "subjectCode": "SUB999",
  "subjectName": "Evidence Subject",
  "credit": 3
}
```

#### 400 Bad Request

```http
POST http://localhost:5125/api/subjects
Content-Type: application/json

{
  "subjectCode": "",
  "subjectName": "",
  "credit": 0
}
```

Hoặc:

```text
GET http://localhost:5125/api/subjects?fields=abcxyz
```

#### 404 Not Found

```text
GET http://localhost:5125/api/courses/999999
```

### Ảnh nên chụp

- 1 ảnh `201`
- 1 ảnh `400`
- 1 ảnh `404`

## 4.10. Docker Deployment - 1.0 điểm

### Mục tiêu cần chứng minh

- Có `Dockerfile`
- Có `docker-compose.yml`
- Database chạy container
- API chạy container
- Có thể mở Swagger từ container

### Evidence tối thiểu

#### A. Chụp file

- `Src/PRN232.LMS.API/Dockerfile`
- `Src/docker-compose.yml`

#### B. Chụp config compose

Chạy:

```powershell
docker compose -f Src\docker-compose.yml config
```

#### C. Chạy thật nếu Docker Desktop đang bật

```powershell
docker compose -f Src\docker-compose.yml up -d --build
docker compose -f Src\docker-compose.yml ps
```

#### D. Mở Swagger container

```text
http://localhost:8080/swagger
```

### Ảnh nên chụp

- 1 ảnh file Docker
- 1 ảnh terminal `docker compose config`
- 1 ảnh terminal `docker compose ps`
- 1 ảnh Swagger mở bằng `localhost:8080`

### Lưu ý

Nếu `docker compose up` lỗi vì Docker daemon chưa chạy, hãy mở Docker Desktop trước rồi chạy lại.

## 4.11. Swagger/OpenAPI - 0.5 điểm

### Cần chụp

- Trang Swagger UI mở được
- Có đủ endpoint
- Có request body cho `POST/PUT`
- Có response/status code docs

### Ảnh gợi ý

- 1 ảnh overview Swagger
- 1 ảnh mở chi tiết 1 endpoint `POST`
- 1 ảnh mở chi tiết 1 endpoint `GET`

## 4.12. Code Quality - 1.1 điểm

Mục này hơi khó chụp hoàn toàn bằng API response, nên nên dùng evidence kết hợp:

### A. Build sạch

Chạy:

```powershell
dotnet build d:\PRN232\LAB1\Src\PRN232.LMS.sln
```

Chụp ảnh terminal có dòng:

- `Build succeeded.`
- `0 Warning(s)`
- `0 Error(s)`

### B. Cấu trúc code rõ ràng

Chụp Solution Explorer để thấy:

- Controllers
- Services
- Repositories
- Models tách rõ

### C. Validation và error handling cơ bản

Chụp 1 request lỗi `400` để chứng minh có validate.

## 5. Bộ test nhanh nên chạy trong Swagger

Nếu bạn muốn demo nhanh trước giảng viên, nên chuẩn bị sẵn các request sau:

```text
GET    /api/semesters
GET    /api/semesters/1
GET    /api/subjects?fields=subjectId,subjectCode,subjectName
GET    /api/students?search=student001
GET    /api/enrollments?page=2&size=5
GET    /api/courses?expand=semester,subject,enrollments
GET    /api/courses/999999
POST   /api/subjects
PUT    /api/subjects/{id}
DELETE /api/subjects/{id}
```

## 6. Thứ tự chụp evidence đề xuất

Để đỡ sót, bạn nên chụp theo thứ tự này:

1. Cấu trúc solution và tên project
2. `dotnet build` thành công
3. SQL schema và seed counts
4. Swagger overview
5. `GET list` với `search/sort/paging/fields/expand`
6. `GET by ID` thành công
7. `404 Not Found`
8. `POST` thành công `201`
9. `POST` lỗi `400`
10. Docker files
11. `docker compose config`
12. `docker compose up` và Swagger container

## 7. Checklist evidence ngắn gọn

Trước khi nộp, tự check lại bạn đã có đủ ảnh sau chưa:

- [ ] Ảnh structure 3-layer
- [ ] Ảnh naming convention
- [ ] Ảnh build success
- [ ] Ảnh DB schema
- [ ] Ảnh seed counts
- [ ] Ảnh Swagger overview
- [ ] Ảnh GET list có search
- [ ] Ảnh GET list có sort
- [ ] Ảnh GET list có paging
- [ ] Ảnh GET list có fields
- [ ] Ảnh GET list có expand
- [ ] Ảnh GET by ID thành công
- [ ] Ảnh 404
- [ ] Ảnh 201
- [ ] Ảnh 400
- [ ] Ảnh Dockerfile + compose
- [ ] Ảnh docker compose chạy được

## 8. Gợi ý nói khi demo

Bạn có thể nói rất ngắn như sau:

`Bài của em tách 3 layer rõ ràng giữa API, Services, Repositories. Database có đủ 5 bảng và seed đủ số lượng theo rubric. API đã có đủ CRUD cho 5 resource, route theo RESTful, GET list hỗ trợ search, sort, paging, fields, expand; GET by ID có trả related data và 404 khi không tồn tại. Response dùng wrapper thống nhất, có Swagger và có cấu hình Docker để chạy API cùng SQL Server.`

## 9. File vừa tạo

File hướng dẫn này nằm tại:

- [Evidence_Guide_Checklist.md](/d:/PRN232/LAB1/Document/Evidence_Guide_Checklist.md)
