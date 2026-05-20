namespace PRN232.LMS.Services.Models.BusinessModels;

public class EnrollmentBusinessModel
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrollDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public StudentBusinessModel? Student { get; set; }
    public CourseBusinessModel? Course { get; set; }
}
