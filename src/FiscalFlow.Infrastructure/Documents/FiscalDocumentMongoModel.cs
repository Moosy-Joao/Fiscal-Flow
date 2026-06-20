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

    [BsonElement("status")]
    public string Status { get; init; } = string.Empty;

    [BsonElement("receivedAtUtc")]
    public DateTime ReceivedAtUtc { get; init; }

    [BsonElement("processedAtUtc")]
    [BsonIgnoreIfNull]
    public DateTime? ProcessedAtUtc { get; init; }

    [BsonElement("failureReason")]
    [BsonIgnoreIfNull]
    public string? FailureReason { get; init; }
}