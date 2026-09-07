using System.ComponentModel.DataAnnotations;

namespace AI.SupportTriage.Api.Contracts;

public sealed class TriageRequest
{
    [Required]
    [MaxLength(4000)]
    public string Message { get; init; } = string.Empty;
}
