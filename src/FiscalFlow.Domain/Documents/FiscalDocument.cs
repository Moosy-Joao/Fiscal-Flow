using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FiscalFlow.Domain.Documents;

public sealed class FiscalDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; init; } = ObjectId.GenerateNewId().ToString();

    [BsonRequired]
    public string TenantId { get; init; } = string.Empty;

    [BsonRequired]
    public string ExternalDocumentId { get; init; } = string.Empty;

    [BsonRequired]
    public string PayloadJson { get; init; } = string.Empty;

    [BsonRequired]
    public DocumentProcessingStatus Status { get; set; } = DocumentProcessingStatus.Received;

    [BsonRequired]
    public DateTimeOffset ReceivedAtUtc { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ProcessedAtUtc { get; set; }
}
