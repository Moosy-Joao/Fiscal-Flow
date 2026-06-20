namespace FiscalFlow.Application.Documents;

public sealed class ListFiscalDocumentsService
{
    private readonly IFiscalDocumentRepository _repository;

    public ListFiscalDocumentsService(
        IFiscalDocumentRepository repository)
    {
        _repository = repository;
    }

    public Task<PagedResult<FiscalDocumentDetails>> ExecuteAsync(
        ListFiscalDocumentsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentException.ThrowIfNullOrWhiteSpace(
            query.TenantId);

        if (query.Page < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(query),
                "A página deve ser maior ou igual a 1.");
        }

        if (query.PageSize is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(query),
                "O tamanho da página deve estar entre 1 e 100.");
        }

        var normalizedQuery = query with
        {
            TenantId = query.TenantId.Trim()
        };

        return _repository.ListAsync(
            normalizedQuery,
            cancellationToken);
    }
}