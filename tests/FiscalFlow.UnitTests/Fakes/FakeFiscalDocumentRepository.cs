using FiscalFlow.Application.Documents;
using FiscalFlow.Domain.Documents;

namespace FiscalFlow.UnitTests.Fakes;

internal sealed class FakeFiscalDocumentRepository
    : IFiscalDocumentRepository
{
    public List<FiscalDocument> Documents { get; } = [];

    public Task InsertAsync(
        FiscalDocument document,
        CancellationToken cancellationToken = default)
    {
        Documents.Add(document);

        return Task.CompletedTask;
    }

    public Task<FiscalDocumentDetails?> FindByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var document = Documents.SingleOrDefault(
            item => item.Id == id);

        if (document is null)
        {
            return Task.FromResult<
                FiscalDocumentDetails?>(null);
        }

        var details = new FiscalDocumentDetails(
            document.Id,
            document.TenantId,
            document.ExternalDocumentId,
            document.Status.ToString(),
            document.ReceivedAtUtc,
            document.ProcessedAtUtc,
            document.FailureReason);

        return Task.FromResult<
            FiscalDocumentDetails?>(details);
    }

    public Task<FiscalDocument?> FindDomainByIdAsync(
    Guid id,
    CancellationToken cancellationToken = default)
    {
        var document = Documents.SingleOrDefault(
            item => item.Id == id);

        return Task.FromResult(document);
    }

    public Task UpdateAsync(
        FiscalDocument document,
        CancellationToken cancellationToken = default)
    {
        var index = Documents.FindIndex(
            item => item.Id == document.Id);

        if (index < 0)
        {
            throw new InvalidOperationException(
                "Documento não encontrado.");
        }

        Documents[index] = document;

        return Task.CompletedTask;
         }

        public Task<PagedResult<FiscalDocumentDetails>> ListAsync(
    ListFiscalDocumentsQuery query,
    CancellationToken cancellationToken = default)
    {
        IEnumerable<FiscalDocument> filteredDocuments =
            Documents.Where(
                document =>
                    document.TenantId == query.TenantId);

        if (query.Status is not null)
        {
            filteredDocuments =
                filteredDocuments.Where(
                    document =>
                        document.Status == query.Status.Value);
        }

        var totalItems =
            filteredDocuments.LongCount();

        var items = filteredDocuments
            .OrderByDescending(
                document => document.ReceivedAtUtc)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(document =>
                new FiscalDocumentDetails(
                    document.Id,
                    document.TenantId,
                    document.ExternalDocumentId,
                    document.Status.ToString(),
                    document.ReceivedAtUtc,
                    document.ProcessedAtUtc,
                    document.FailureReason))
            .ToList();

        var result =
            new PagedResult<FiscalDocumentDetails>(
                items,
                query.Page,
                query.PageSize,
                totalItems);

        return Task.FromResult(result);
    }
}