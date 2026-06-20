using FiscalFlow.Domain.Documents;

namespace FiscalFlow.Application.Documents;

public sealed class CreateFiscalDocumentService
{
    private readonly IFiscalDocumentRepository _repository;

    public CreateFiscalDocumentService(
        IFiscalDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<CreateFiscalDocumentResult> ExecuteAsync(
        CreateFiscalDocumentCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var document = new FiscalDocument(
            command.TenantId,
            command.ExternalDocumentId);

        await _repository.InsertAsync(
            document,
            cancellationToken);

        return new CreateFiscalDocumentResult(
            document.Id,
            document.TenantId,
            document.ExternalDocumentId,
            document.Status.ToString(),
            document.ReceivedAtUtc);
    }
}