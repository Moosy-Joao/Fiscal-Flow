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
        var alreadyExists = Documents.Any(
            savedDocument =>
                savedDocument.TenantId
                    == document.TenantId
                && savedDocument.ExternalDocumentId
                    == document.ExternalDocumentId);

        if (alreadyExists)
        {
            throw new DuplicateFiscalDocumentException(
                document.TenantId,
                document.ExternalDocumentId);
        }

        Documents.Add(document);

        return Task.CompletedTask;
    }

    public Task<FiscalDocumentDetails?> FindByIdAsync(
        Guid id,
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        var document = Documents.SingleOrDefault(
            item =>
                item.Id == id
                && item.TenantId == tenantId);

        if (document is null)
        {
            return Task.FromResult<
                FiscalDocumentDetails?>(null);
        }

        var details = MapToDetails(document);

        return Task.FromResult<
            FiscalDocumentDetails?>(details);
    }

    public Task<FiscalDocument?> FindDomainByIdAsync(
        Guid id,
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        var document = Documents.SingleOrDefault(
            item =>
                item.Id == id
                && item.TenantId == tenantId);

        return Task.FromResult(document);
    }

    public Task UpdateAsync(
        FiscalDocument document,
        CancellationToken cancellationToken = default)
    {
        var index = Documents.FindIndex(
            item =>
                item.Id == document.Id
                && item.TenantId
                    == document.TenantId);

        if (index < 0)
        {
            throw new InvalidOperationException(
                "Documento não encontrado.");
        }

        Documents[index] = document;

        return Task.CompletedTask;
    }

    public Task<PagedResult<FiscalDocumentDetails>>
        ListAsync(
            ListFiscalDocumentsQuery query,
            CancellationToken cancellationToken = default)
    {
        IEnumerable<FiscalDocument> filteredDocuments =
            Documents.Where(
                document =>
                    document.TenantId
                        == query.TenantId);

        if (query.Status is not null)
        {
            filteredDocuments =
                filteredDocuments.Where(
                    document =>
                        document.Status
                            == query.Status.Value);
        }

        var totalItems =
            filteredDocuments.LongCount();

        var items = filteredDocuments
            .OrderByDescending(
                document =>
                    document.ReceivedAtUtc)
            .Skip(
                (query.Page - 1)
                * query.PageSize)
            .Take(query.PageSize)
            .Select(MapToDetails)
            .ToList();

        var result =
            new PagedResult<FiscalDocumentDetails>(
                items,
                query.Page,
                query.PageSize,
                totalItems);

        return Task.FromResult(result);
    }

    private static FiscalDocumentDetails MapToDetails(
        FiscalDocument document)
    {
        return new FiscalDocumentDetails(
            document.Id,
            document.TenantId,
            document.ExternalDocumentId,
            document.Status.ToString(),
            document.ReceivedAtUtc,
            document.ProcessedAtUtc,
            document.FailureReason);
    }

    public Task<FiscalDocumentDetails?>
    FindByExternalDocumentIdAsync(
        string tenantId,
        string externalDocumentId,
        CancellationToken cancellationToken = default)
    {
        var document = Documents.SingleOrDefault(
            item =>
                item.TenantId == tenantId
                && item.ExternalDocumentId
                    == externalDocumentId);

        if (document is null)
        {
            return Task.FromResult<
                FiscalDocumentDetails?>(null);
        }

        return Task.FromResult<
            FiscalDocumentDetails?>(
                MapToDetails(document));
    }
}