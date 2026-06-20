using System.ComponentModel.DataAnnotations;

namespace FiscalFlow.Api.Contracts;

public sealed class CreateFiscalDocumentRequest
{
    [Required]
    public string TenantId { get; init; } = string.Empty;

    [Required]
    public string ExternalDocumentId { get; init; } =
        string.Empty;
}