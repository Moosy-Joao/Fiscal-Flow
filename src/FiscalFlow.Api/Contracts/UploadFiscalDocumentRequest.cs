using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FiscalFlow.Api.Contracts;

public sealed class UploadFiscalDocumentRequest
{
    [Required]
    public string ExternalDocumentId { get; init; } =
        string.Empty;

    [Required]
    public IFormFile File { get; init; } =
        null!;
}
