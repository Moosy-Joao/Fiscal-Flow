using System.ComponentModel.DataAnnotations;

namespace FiscalFlow.Api.Contracts;

public sealed class UpdateFiscalDocumentStatusRequest
{
    [Required]
    public string Status { get; init; } = string.Empty;

    public string? FailureReason { get; init; }
}