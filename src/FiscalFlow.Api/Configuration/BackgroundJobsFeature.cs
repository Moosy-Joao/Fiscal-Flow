using FiscalFlow.Infrastructure.MongoDb;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using MongoDB.Driver;

namespace FiscalFlow.Api.Configuration;

internal static class BackgroundJobsFeature
{
    public static bool AddBackgroundJobsFeature(
        this WebApplicationBuilder builder,
        MongoDbOptions mongoDbOptions)
    {
        var options = builder.Configuration
            .GetSection(BackgroundJobsOptions.SectionName)
            .Get<BackgroundJobsOptions>()
            ?? new BackgroundJobsOptions();

        if (!options.Enabled)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(options.DatabaseName)
            || string.IsNullOrWhiteSpace(options.CollectionPrefix)
            || string.IsNullOrWhiteSpace(options.FailedRetryCron)
            || string.IsNullOrWhiteSpace(options.TimedOutProcessingCron)
            || options.WorkerCount <= 0
            || options.FailedBatchSize <= 0
            || options.MaximumFailedAttempts <= 0
            || options.TimedOutProcessingBatchSize <= 0
            || options.ProcessingTimeoutMinutes <= 0)
        {
            throw new InvalidOperationException(
                "A seção BackgroundJobs não foi configurada corretamente.");
        }

        var mongoClient = new MongoClient(
            mongoDbOptions.ConnectionString);

        builder.Services.AddSingleton(options);

        builder.Services.AddHangfire(configuration =>
            configuration
                .SetDataCompatibilityLevel(
                    CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseMongoStorage(
                    mongoClient,
                    options.DatabaseName,
                    new MongoStorageOptions
                    {
                        Prefix = options.CollectionPrefix,
                        CheckConnection = true,
                        MigrationOptions =
                            new MongoMigrationOptions
                            {
                                MigrationStrategy =
                                    new MigrateMongoMigrationStrategy(),
                                BackupStrategy =
                                    new CollectionMongoBackupStrategy()
                            }
                    }));

        builder.Services.AddHangfireServer(serverOptions =>
        {
            serverOptions.WorkerCount = options.WorkerCount;
        });

        return true;
    }
}
