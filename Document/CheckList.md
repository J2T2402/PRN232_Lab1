Checklist	Điểm tối đa
"3-layer Architecture
- API / Services / Repositories tách rõ; Controller không chứa business logic; Repository không chứa business logic."	1.2
"Project Naming Convention
- Đúng format PRN232.[ProjectName].API / Services / Repositories."	0.4
"DB Schema & Seed Data
- Đủ bảng Semester, Course, Subject, Student, Enrollment; seed tối thiểu 5 semesters, 50 students, 10 subjects, 20 courses, 500 enrollments."	0.8
"4 Model Types
- Có Entity, Business, Request, Response model; không trả Entity trực tiếp; không dùng Request/Response trong Repository."	1.0
"RESTful Endpoint Naming
- Endpoint resource-based, plural nouns; tránh get/create trong URL."	0.7
"GET by ID
- Trả đủ related data khi cần, tránh circular reference, trả 404 nếu không tồn tại."	0.6
"List API Capabilities
- List API hỗ trợ search/filter, sort, paging, fields selection, expand related resources."	1.5
"Pagination Metadata
- Response có page, pageSize, totalItems, totalPages."	0.4
"Response Format & HTTP Status
- Format consistent success/message/data/errors; status 200/201/400/404/500 phù hợp."	0.8
"Docker Deployment
- Database chạy Docker Desktop; API chạy container; có Dockerfile & docker-compose.yml; demo docker compose chạy được."	1.0
"Swagger/OpenAPI
- Có Swagger, listing endpoint, test API, request/response docs, HTTP status code docs."	0.5
"Code Quality
- Code clean, naming rõ, ít duplicate, xử lý lỗi cơ bản, cấu trúc dễ đọc và bảo trì."	1.1