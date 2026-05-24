using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.Models.Requests;

public sealed class CourseUpsertRequest
{
    [Required]
    [MaxLength(100)]
    public string CourseName { get; init; } = string.Empty;

    public int? SemesterId { get; init; }
}
