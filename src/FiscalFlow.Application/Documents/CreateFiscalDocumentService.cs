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

        ArgumentException.ThrowIfNullOrWhiteSpace(
            command.XmlContent);

        var tenantId =
            command.TenantId.Trim();

        var externalDocumentId =
            command.ExternalDocumentId.Trim();

        var xmlContent =
            command.XmlContent.Trim();

        var existingDocument =
            await _repository
                .FindByExternalDocumentIdAsync(
                    tenantId,
                    externalDocumentId,
                    cancellationToken);

        if (existingDocument is not null)
        {
            await PublishIfPendingAsync(
                existingDocument,
                cancellationToken);

            return MapToResult(
                existingDocument,
                wasCreated: false);
        }

        var document = new FiscalDocument(
            tenantId,
            externalDocumentId,
            xmlContent: xmlContent);

        try
        {
            await _repository.InsertAsync(
                document,
                cancellationToken);

            await PublishAsync(
                document.Id,
                document.TenantId,
                document.ExternalDocumentId,
                document.ReceivedAtUtc,
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

            await PublishIfPendingAsync(
                existingDocument,
                cancellationToken);

            return MapToResult(
                existingDocument,
                wasCreated: false);
        }
    }

    private Task PublishIfPendingAsync(
        FiscalDocumentDetails document,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(
                document.Status,
                DocumentProcessingStatus.Received.ToString(),
                StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        return PublishAsync(
            document.Id,
            document.TenantId,
            document.ExternalDocumentId,
            document.ReceivedAtUtc,
            cancellationToken);
    }

    private Task PublishAsync(
        Guid documentId,
        string tenantId,
        string externalDocumentId,
        DateTimeOffset receivedAtUtc,
        CancellationToken cancellationToken)
    {
        var message =
            new FiscalDocumentReceivedMessage(
                documentId,
                tenantId,
                externalDocumentId,
                receivedAtUtc,
                Guid.NewGuid());

        return _publisher.PublishAsync(
            message,
            cancellationToken);
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
