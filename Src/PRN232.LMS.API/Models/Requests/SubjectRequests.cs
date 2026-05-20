using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.Models.Requests;

public class CreateSubjectRequest
{
    [Required]
    [MaxLength(20)]
    public string SubjectCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string SubjectName { get; set; } = string.Empty;

    [Range(1, 10)]
    public int Credit { get; set; }
}

public class UpdateSubjectRequest : CreateSubjectRequest;
