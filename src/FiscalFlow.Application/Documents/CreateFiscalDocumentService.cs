using FiscalFlow.Application.Messaging;
using FiscalFlow.Domain.Documents;

namespace FiscalFlow.Application.Documents;

public sealed class CreateFiscalDocumentService
{
    private readonly IFiscalDocumentRepository
        _repository;

    private readonly IFiscalDocumentReceivedPublisher
        _publisher;

    public CreateFiscalDocumentService(
        IFiscalDocumentRepository repository,
        IFiscalDocumentReceivedPublisher publisher)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(publisher);

        _repository = repository;
        _publisher = publisher;
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

            var message =
                new FiscalDocumentReceivedMessage(
                    document.Id,
                    document.TenantId,
                    document.ExternalDocumentId,
                    document.ReceivedAtUtc,
                    Guid.NewGuid());

            await _publisher.PublishAsync(
                message,
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