using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.Models.Requests;

public sealed class StudentUpsertRequest
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; init; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

[Phone]
public string? Phone { get; init; }

[RegularExpression(@"^[A-Z]{2}\d{5}$", ErrorMessage = "StudentCode must be FPTU style, e.g. SE19886")]
public string? StudentCode { get; init; }
    public DateTime DateOfBirth { get; init; }
}
