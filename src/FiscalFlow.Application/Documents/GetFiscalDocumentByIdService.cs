namespace FiscalFlow.Application.Documents;

public sealed class GetFiscalDocumentByIdService
{
    private readonly IFiscalDocumentRepository _repository;

    public GetFiscalDocumentByIdService(
        IFiscalDocumentRepository repository)
    {
        _repository = repository;
    }

    public Task<FiscalDocumentDetails?> ExecuteAsync(
        Guid id,
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "O ID do documento não pode ser vazio.",
                nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            tenantId);

        return _repository.FindByIdAsync(
            id,
            tenantId.Trim(),
            cancellationToken);
    }
}