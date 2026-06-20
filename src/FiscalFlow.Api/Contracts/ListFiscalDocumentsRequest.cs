using System.ComponentModel.DataAnnotations;

namespace FiscalFlow.Api.Contracts;

public sealed class ListFiscalDocumentsRequest
{
    public string? Status { get; init; }

    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 10;
}