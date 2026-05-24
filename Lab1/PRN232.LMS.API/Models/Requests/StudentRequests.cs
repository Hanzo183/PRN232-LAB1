using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.Models.Requests;

public sealed class StudentUpsertRequest
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; init; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Email { get; init; } = string.Empty;

    public DateTime DateOfBirth { get; init; }
}
