using MongoDB.Bson;
using MongoDB.Driver;

namespace FiscalFlow.Infrastructure.MongoDb;

public sealed class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public string DatabaseName { get; }

    public MongoDbContext(MongoDbOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            throw new InvalidOperationException(
                "A connection string do MongoDB não foi configurada.");
        }

        if (string.IsNullOrWhiteSpace(options.DatabaseName))
        {
            throw new InvalidOperationException(
                "O nome do banco MongoDB não foi configurado.");
        }

        var clientSettings =
            MongoClientSettings.FromConnectionString(
                options.ConnectionString);

        clientSettings.ServerSelectionTimeout =
            TimeSpan.FromSeconds(5);

        var client = new MongoClient(clientSettings);

        DatabaseName = options.DatabaseName;
        _database = client.GetDatabase(DatabaseName);
    }

    public IMongoCollection<TDocument> GetCollection<TDocument>(
    string collectionName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            collectionName);

        return _database.GetCollection<TDocument>(
            collectionName);
    }

    public async Task PingAsync(
        CancellationToken cancellationToken = default)
    {
        var command = new BsonDocument("ping", 1);

        await _database.RunCommandAsync<BsonDocument>(
            command,
            cancellationToken: cancellationToken);
    }
}