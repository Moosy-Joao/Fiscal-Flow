namespace FiscalFlow.Domain.Documents;

public sealed class FiscalDocument
{
    public Guid Id { get; }
    public string TenantId { get; }
    public string ExternalDocumentId { get; }
    public DocumentProcessingStatus Status { get; private set; }
    public DateTimeOffset ReceivedAtUtc { get; }
    public DateTimeOffset? ProcessedAtUtc { get; private set; }
    public string? FailureReason { get; private set; }

    public FiscalDocument(
        string tenantId,
        string externalDocumentId,
        DateTimeOffset? receivedAtUtc = null)
        : this(
            Guid.NewGuid(),
            tenantId,
            externalDocumentId,
            DocumentProcessingStatus.Received,
            receivedAtUtc ?? DateTimeOffset.UtcNow,
            null,
            null)
    {
    }

    private FiscalDocument(
        Guid id,
        string tenantId,
        string externalDocumentId,
        DocumentProcessingStatus status,
        DateTimeOffset receivedAtUtc,
        DateTimeOffset? processedAtUtc,
        string? failureReason)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "O ID do documento não pode ser vazio.",
                nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        ArgumentException.ThrowIfNullOrWhiteSpace(
            externalDocumentId);

        if (status == DocumentProcessingStatus.Processed
            && processedAtUtc is null)
        {
            throw new ArgumentException(
                "Um documento processado precisa ter uma data de processamento.",
                nameof(processedAtUtc));
        }

        if (status == DocumentProcessingStatus.Failed
            && string.IsNullOrWhiteSpace(failureReason))
        {
            throw new ArgumentException(
                "Um documento com falha precisa ter um motivo.",
                nameof(failureReason));
        }

        Id = id;
        TenantId = tenantId.Trim();
        ExternalDocumentId = externalDocumentId.Trim();
        Status = status;
        ReceivedAtUtc = receivedAtUtc;
        ProcessedAtUtc = processedAtUtc;
        FailureReason = failureReason?.Trim();
    }

    public static FiscalDocument Rehydrate(
        Guid id,
        string tenantId,
        string externalDocumentId,
        DocumentProcessingStatus status,
        DateTimeOffset receivedAtUtc,
        DateTimeOffset? processedAtUtc,
        string? failureReason)
    {
        return new FiscalDocument(
            id,
            tenantId,
            externalDocumentId,
            status,
            receivedAtUtc,
            processedAtUtc,
            failureReason);
    }

    public void MarkAsProcessing()
    {
        if (Status is not DocumentProcessingStatus.Received
            and not DocumentProcessingStatus.Failed)
        {
            throw new InvalidOperationException(
                $"Não é possível iniciar o processamento de um documento com status {Status}.");
        }

        Status = DocumentProcessingStatus.Processing;
        ProcessedAtUtc = null;
        FailureReason = null;
    }

    public void MarkAsProcessed(
        DateTimeOffset? processedAtUtc = null)
    {
        if (Status != DocumentProcessingStatus.Processing)
        {
            throw new InvalidOperationException(
                "O documento precisa estar em processamento antes de ser concluído.");
        }

        Status = DocumentProcessingStatus.Processed;
        ProcessedAtUtc =
            processedAtUtc ?? DateTimeOffset.UtcNow;

        FailureReason = null;
    }

    public void MarkAsFailed(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if (Status == DocumentProcessingStatus.Processed)
        {
            throw new InvalidOperationException(
                "Um documento processado não pode ser marcado como falha.");
        }

        Status = DocumentProcessingStatus.Failed;
        ProcessedAtUtc = null;
        FailureReason = reason.Trim();
    }
}