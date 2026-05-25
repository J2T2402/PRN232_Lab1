using System.Text.Json.Serialization;

namespace PRN232.LMS.API.Models.Responses;

public class SemesterResponse
{
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<CourseResponse>? Courses { get; set; }
}
