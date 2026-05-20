namespace PRN232.LMS.Services.Models.BusinessModels;

public class SubjectBusinessModel
{
    public int SubjectId { get; set; }
    public string SubjectCode { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public int Credit { get; set; }
    public IReadOnlyList<CourseBusinessModel>? Courses { get; set; }
}
