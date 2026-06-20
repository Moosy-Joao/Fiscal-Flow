using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FiscalFlow.Domain.Documents;

public sealed class IdempotencyRecord
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; init; } = ObjectId.GenerateNewId().ToString();

    [BsonRequired]
    public string TenantId { get; init; } = string.Empty;

    [BsonRequired]
    public string IdempotencyKey { get; init; } = string.Empty;

    [BsonRequired]
    public string DocumentId { get; init; } = string.Empty;

    [BsonRequired]
    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;
}
