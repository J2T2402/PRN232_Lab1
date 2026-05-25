using System.Text.Json.Serialization;

namespace PRN232.LMS.API.Models.Responses;

public class StudentResponse
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<EnrollmentResponse>? Enrollments { get; set; }
}
