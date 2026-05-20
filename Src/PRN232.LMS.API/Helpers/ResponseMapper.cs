using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Models.BusinessModels;

namespace PRN232.LMS.API.Helpers;

public static class ResponseMapper
{
    public static SemesterResponse ToSemesterResponse(SemesterBusinessModel model)
        => new()
        {
            SemesterId = model.SemesterId,
            SemesterName = model.SemesterName,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            Courses = model.Courses?.Select(ToCourseSummaryResponse).ToList()
        };

    public static SubjectResponse ToSubjectResponse(SubjectBusinessModel model)
        => new()
        {
            SubjectId = model.SubjectId,
            SubjectCode = model.SubjectCode,
            SubjectName = model.SubjectName,
            Credit = model.Credit,
            Courses = model.Courses?.Select(ToCourseSummaryResponse).ToList()
        };

    public static CourseResponse ToCourseResponse(CourseBusinessModel model)
        => new()
        {
            CourseId = model.CourseId,
            CourseName = model.CourseName,
            SemesterId = model.SemesterId,
            SubjectId = model.SubjectId,
            Semester = model.Semester is null ? null : ToSemesterSummaryResponse(model.Semester),
            Subject = model.Subject is null ? null : ToSubjectSummaryResponse(model.Subject),
            Enrollments = model.Enrollments?.Select(ToEnrollmentForCourseResponse).ToList()
        };

    public static StudentResponse ToStudentResponse(StudentBusinessModel model)
        => new()
        {
            StudentId = model.StudentId,
            FullName = model.FullName,
            Email = model.Email,
            DateOfBirth = model.DateOfBirth,
            Enrollments = model.Enrollments?.Select(ToEnrollmentForStudentResponse).ToList()
        };

    public static EnrollmentResponse ToEnrollmentResponse(EnrollmentBusinessModel model)
        => new()
        {
            EnrollmentId = model.EnrollmentId,
            StudentId = model.StudentId,
            CourseId = model.CourseId,
            EnrollDate = model.EnrollDate,
            Status = model.Status,
            Student = model.Student is null ? null : ToStudentSummaryResponse(model.Student),
            Course = model.Course is null ? null : ToCourseSummaryResponse(model.Course)
        };

    private static SemesterResponse ToSemesterSummaryResponse(SemesterBusinessModel model)
        => new()
        {
            SemesterId = model.SemesterId,
            SemesterName = model.SemesterName,
            StartDate = model.StartDate,
            EndDate = model.EndDate
        };

    private static SubjectResponse ToSubjectSummaryResponse(SubjectBusinessModel model)
        => new()
        {
            SubjectId = model.SubjectId,
            SubjectCode = model.SubjectCode,
            SubjectName = model.SubjectName,
            Credit = model.Credit
        };

    private static CourseResponse ToCourseSummaryResponse(CourseBusinessModel model)
        => new()
        {
            CourseId = model.CourseId,
            CourseName = model.CourseName,
            SemesterId = model.SemesterId,
            SubjectId = model.SubjectId
        };

    private static StudentResponse ToStudentSummaryResponse(StudentBusinessModel model)
        => new()
        {
            StudentId = model.StudentId,
            FullName = model.FullName,
            Email = model.Email,
            DateOfBirth = model.DateOfBirth
        };

    private static EnrollmentResponse ToEnrollmentForCourseResponse(EnrollmentBusinessModel model)
        => new()
        {
            EnrollmentId = model.EnrollmentId,
            StudentId = model.StudentId,
            CourseId = model.CourseId,
            EnrollDate = model.EnrollDate,
            Status = model.Status,
            Student = model.Student is null ? null : ToStudentSummaryResponse(model.Student)
        };

    private static EnrollmentResponse ToEnrollmentForStudentResponse(EnrollmentBusinessModel model)
        => new()
        {
            EnrollmentId = model.EnrollmentId,
            StudentId = model.StudentId,
            CourseId = model.CourseId,
            EnrollDate = model.EnrollDate,
            Status = model.Status,
            Course = model.Course is null ? null : ToCourseSummaryResponse(model.Course)
        };
}
