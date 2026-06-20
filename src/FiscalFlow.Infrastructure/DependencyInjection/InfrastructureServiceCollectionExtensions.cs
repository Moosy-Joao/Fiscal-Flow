using FiscalFlow.Application.Documents;
using FiscalFlow.Infrastructure.Messaging;
using FiscalFlow.Infrastructure.Persistence;
using FiscalFlow.Infrastructure.Processing;
using Hangfire;
using Hangfire.Mongo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace FiscalFlow.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddFiscalFlowInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoOptions>(configuration.GetSection(MongoOptions.SectionName));
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));

        var mongoOptions = configuration.GetSection(MongoOptions.SectionName).Get<MongoOptions>() ?? new MongoOptions();

        services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoOptions.ConnectionString));

        services.AddScoped<IFiscalDocumentRepository, FiscalDocumentRepository>();
        services.AddScoped<IIdempotencyRepository, IdempotencyRepository>();
        services.AddScoped<IDocumentDispatchQueue, RabbitMqDispatchQueue>();
        services.AddScoped<IFiscalDocumentService, FiscalDocumentService>();
        services.AddScoped<IFiscalDocumentProcessor, FiscalDocumentProcessor>();
        services.AddScoped<FiscalDocumentProcessingJob>();

        services.AddHangfire(config =>
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseMongoStorage(mongoOptions.ConnectionString, mongoOptions.DatabaseName + "_hangfire", new MongoStorageOptions
                {
                    CheckConnection = false,
                    Prefix = "hangfire"
                });
        });

        services.AddHangfireServer();
        services.AddScoped<IBackgroundJobScheduler, HangfireBackgroundJobScheduler>();

        return services;
    }
}
