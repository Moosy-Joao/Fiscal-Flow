using FiscalFlow.Infrastructure.MongoDb;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using MongoDB.Driver;

namespace FiscalFlow.Api.Configuration;

internal static class BackgroundJobsFeature
{
    public static bool AddBackgroundJobsFeature(this WebApplicationBuilder builder,MongoDbOptions mongoDbOptions)
    {
        var options = builder.Configuration.GetSection(BackgroundJobsOptions.SectionName).Get<BackgroundJobsOptions>() ?? new BackgroundJobsOptions();
        if (!options.Enabled) return false;

        var mongoClient = new MongoClient(mongoDbOptions.ConnectionString);
        builder.Services.AddSingleton(options);

        GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute
        {
            Attempts = options.MaximumFailedAttempts
        });

        builder.Services.AddHangfire(configuration => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseMongoStorage(mongoClient, options.DatabaseName, new MongoStorageOptions { Prefix = options.CollectionPrefix, CheckConnection = true }));

        builder.Services.AddHangfireServer(x => x.WorkerCount = options.WorkerCount);
        return true;
    }
}