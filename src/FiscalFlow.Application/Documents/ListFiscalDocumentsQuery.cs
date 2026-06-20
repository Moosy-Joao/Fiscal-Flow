using FiscalFlow.Domain.Documents;

namespace FiscalFlow.Application.Documents;

public sealed record ListFiscalDocumentsQuery(
    string TenantId,
    DocumentProcessingStatus? Status,
    int Page,
    int PageSize);