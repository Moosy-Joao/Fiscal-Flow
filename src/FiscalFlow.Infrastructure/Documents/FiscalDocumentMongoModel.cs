using MongoDB.Bson.Serialization.Attributes;

namespace FiscalFlow.Infrastructure.Documents;

internal sealed class FiscalDocumentMongoModel
{
    [BsonId]
    public string Id { get; init; } = string.Empty;

    [BsonElement("tenantId")]
    public string TenantId { get; init; } = string.Empty;

    [BsonElement("externalDocumentId")]
    public string ExternalDocumentId { get; init; } =
        string.Empty;

    [BsonElement("xmlContent")]
    [BsonIgnoreIfNull]
    public string? XmlContent { get; init; }

    [BsonElement("accessKey")]
    [BsonIgnoreIfNull]
    public string? AccessKey { get; init; }

    [BsonElement("issuerDocument")]
    [BsonIgnoreIfNull]
    public string? IssuerDocument { get; init; }

    [BsonElement("issuerName")]
    [BsonIgnoreIfNull]
    public string? IssuerName { get; init; }

    [BsonElement("recipientDocument")]
    [BsonIgnoreIfNull]
    public string? RecipientDocument { get; init; }

    [BsonElement("recipientName")]
    [BsonIgnoreIfNull]
    public string? RecipientName { get; init; }

    [BsonElement("totalValue")]
    [BsonIgnoreIfNull]
    public decimal? TotalValue { get; init; }

    [BsonElement("issuedAt")]
    [BsonIgnoreIfNull]
    public DateTime? IssuedAt { get; init; }

    [BsonElement("status")]
    public string Status { get; init; } = string.Empty;

    [BsonElement("receivedAtUtc")]
    public DateTime ReceivedAtUtc { get; init; }

    [BsonElement("processingStartedAtUtc")]
    [BsonIgnoreIfNull]
    public DateTime? ProcessingStartedAtUtc { get; init; }

    [BsonElement("processedAtUtc")]
    [BsonIgnoreIfNull]
    public DateTime? ProcessedAtUtc { get; init; }

    [BsonElement("failureReason")]
    [BsonIgnoreIfNull]
    public string? FailureReason { get; init; }

    [BsonElement("reprocessingAttempts")]
    public int ReprocessingAttempts { get; init; }

    [BsonElement("lastReprocessingAtUtc")]
    [BsonIgnoreIfNull]
    public DateTime? LastReprocessingAtUtc { get; init; }
}
