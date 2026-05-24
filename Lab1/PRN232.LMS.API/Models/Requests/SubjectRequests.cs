using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.Models.Requests;

public sealed class SubjectUpsertRequest
{
    [Required]
    [MaxLength(20)]
    public string SubjectCode { get; init; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string SubjectName { get; init; } = string.Empty;

    public int Credit { get; init; }
}
