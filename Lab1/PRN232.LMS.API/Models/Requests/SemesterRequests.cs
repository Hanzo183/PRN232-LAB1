using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.Models.Requests;

public sealed class SemesterUpsertRequest
{
    [Required]
    [MaxLength(100)]
    public string SemesterName { get; init; } = string.Empty;

    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}
