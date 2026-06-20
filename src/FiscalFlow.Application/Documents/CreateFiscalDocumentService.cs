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

        ArgumentException.ThrowIfNullOrWhiteSpace(
            command.TenantId);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            command.ExternalDocumentId);

        var tenantId = command.TenantId.Trim();
        var externalDocumentId =
            command.ExternalDocumentId.Trim();

        var existingDocument =
            await _repository
                .FindByExternalDocumentIdAsync(
                    tenantId,
                    externalDocumentId,
                    cancellationToken);

        if (existingDocument is not null)
        {
            return MapToResult(
                existingDocument,
                wasCreated: false);
        }

        var document = new FiscalDocument(
            tenantId,
            externalDocumentId);

        try
        {
            await _repository.InsertAsync(
                document,
                cancellationToken);

            return new CreateFiscalDocumentResult(
                document.Id,
                document.TenantId,
                document.ExternalDocumentId,
                document.Status.ToString(),
                document.ReceivedAtUtc,
                WasCreated: true);
        }
        catch (DuplicateFiscalDocumentException)
        {
            existingDocument =
                await _repository
                    .FindByExternalDocumentIdAsync(
                        tenantId,
                        externalDocumentId,
                        cancellationToken);

            if (existingDocument is null)
            {
                throw;
            }

            return MapToResult(
                existingDocument,
                wasCreated: false);
        }
    }

    private static CreateFiscalDocumentResult MapToResult(
        FiscalDocumentDetails document,
        bool wasCreated)
    {
        return new CreateFiscalDocumentResult(
            document.Id,
            document.TenantId,
            document.ExternalDocumentId,
            document.Status,
            document.ReceivedAtUtc,
            wasCreated);
    }
}