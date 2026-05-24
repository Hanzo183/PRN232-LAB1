using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.Models.Requests;

public sealed class EnrollmentUpsertRequest
{
    [Required]
    public int StudentId { get; init; }

    [Required]
    public int CourseId { get; init; }

    public DateTime? EnrollDate { get; init; }

    [Required]
    [MaxLength(20)]
    public string Status { get; init; } = string.Empty;
}
