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
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        ArgumentException.ThrowIfNullOrWhiteSpace(externalDocumentId);

        Id = Guid.NewGuid();
        TenantId = tenantId.Trim();
        ExternalDocumentId = externalDocumentId.Trim();
        Status = DocumentProcessingStatus.Received;
        ReceivedAtUtc = receivedAtUtc ?? DateTimeOffset.UtcNow;
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

    public void MarkAsProcessed(DateTimeOffset? processedAtUtc = null)
    {
        if (Status != DocumentProcessingStatus.Processing)
        {
            throw new InvalidOperationException(
                "O documento precisa estar em processamento antes de ser concluído.");
        }

        Status = DocumentProcessingStatus.Processed;
        ProcessedAtUtc = processedAtUtc ?? DateTimeOffset.UtcNow;
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
