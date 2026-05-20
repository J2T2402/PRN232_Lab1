namespace PRN232.LMS.API.Models.Responses;

public class SubjectResponse
{
    public int SubjectId { get; set; }
    public string SubjectCode { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public int Credit { get; set; }
    public IReadOnlyList<CourseResponse>? Courses { get; set; }
}
