using System.Diagnostics;
using FiscalFlow.Api.Configuration;
using FiscalFlow.Api.Health;
using FiscalFlow.Api.Jobs;
using FiscalFlow.Api.Observability;
using FiscalFlow.Api.Tenancy;
using FiscalFlow.Application.Documents;
using FiscalFlow.Application.Documents.Xml;
using FiscalFlow.Infrastructure.Documents;
using FiscalFlow.Infrastructure.MongoDb;
using FiscalFlow.Infrastructure.RabbitMq;
using Hangfire;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Features;

Activity.DefaultIdFormat = ActivityIdFormat.W3C;
Activity.ForceDefaultIdFormat = true;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.AddObservabilityFeature();

var uploadOptions = builder.Configuration
    .GetSection(FiscalDocumentUploadOptions.SectionName)
    .Get<FiscalDocumentUploadOptions>()
    ?? new FiscalDocumentUploadOptions();

if (uploadOptions.MaxFileSizeBytes <= 0)
{
    throw new InvalidOperationException(
        "O limite de upload do XML deve ser maior que zero.");
}

var multipartBodyLengthLimit = checked(
    uploadOptions.MaxFileSizeBytes + 64 * 1024);

builder.Services.AddSingleton(uploadOptions);

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit =
        multipartBodyLengthLimit;
});

var mongoDbOptions = builder.Configuration
    .GetSection(MongoDbOptions.SectionName)
    .Get<MongoDbOptions>();

if (mongoDbOptions is null
    || string.IsNullOrWhiteSpace(
        mongoDbOptions.ConnectionString)
    || string.IsNullOrWhiteSpace(
        mongoDbOptions.DatabaseName))
{
    throw new InvalidOperationException(
        "A seção MongoDb não foi configurada corretamente.");
}

builder.Services.AddSingleton(mongoDbOptions);
builder.Services.AddSingleton<MongoDbContext>();

builder.Services.AddSingleton<
    FiscalDocumentIndexManager>();

builder.Services.AddSingleton<
    FiscalDocumentRepository>();

builder.Services.AddSingleton<IFiscalDocumentRepository>(
    services =>
        services.GetRequiredService<
            FiscalDocumentRepository>());

builder.Services.AddSingleton<IProcessingTimeoutRepository>(
    services =>
        services.GetRequiredService<
            FiscalDocumentRepository>());

builder.Services.AddSingleton<
    IFiscalDocumentXmlParser,
    FiscalDocumentXmlParser>();

builder.Services.AddSingleton<
    FiscalDocumentXmlFileReader>();

var rabbitMqEnabled =
    builder.AddRabbitMqFeature();

var backgroundJobsEnabled =
    builder.AddBackgroundJobsFeature(
        mongoDbOptions);

var healthChecks = builder.Services
    .AddHealthChecks()
    .AddCheck<MongoDbHealthCheck>(
        "mongodb",
        tags: ["ready"]);

if (rabbitMqEnabled)
{
    healthChecks.AddCheck<RabbitMqHealthCheck>(
        "rabbitmq",
        tags: ["ready"]);
}

builder.Services.AddScoped<
    CreateFiscalDocumentService>();

builder.Services.AddScoped<
    ProcessFiscalDocumentService>();

builder.Services.AddScoped<
    RetryDocumentBatchService>();

builder.Services.AddScoped<
    RetryFailedDocumentsJob>();

builder.Services.AddScoped<
    DetectTimedOutProcessingService>();

builder.Services.AddScoped<
    DetectTimedOutProcessingJob>();

builder.Services.AddScoped<
    GetFiscalDocumentByIdService>();

builder.Services.AddScoped<
    UpdateFiscalDocumentStatusService>();

builder.Services.AddScoped<
    ListFiscalDocumentsService>();

builder.Services.AddScoped<TenantContext>();

var app = builder.Build();

if (mongoDbOptions.InitializeIndexes)
{
    var indexManager =
        app.Services.GetRequiredService<
            FiscalDocumentIndexManager>();

    await indexManager.EnsureCreatedAsync();
}

if (rabbitMqEnabled)
{
    var topologyInitializer =
        app.Services.GetRequiredService<
            RabbitMqTopologyInitializer>();

    await topologyInitializer.InitializeAsync();
}

if (backgroundJobsEnabled)
{
    using var scope = app.Services.CreateScope();

    var recurringJobs =
        scope.ServiceProvider.GetRequiredService<
            IRecurringJobManager>();

    var options =
        scope.ServiceProvider.GetRequiredService<
            BackgroundJobsOptions>();

    recurringJobs.AddOrUpdate<RetryFailedDocumentsJob>(
        "retry-failed-documents",
        job => job.ExecuteAsync(),
        options.FailedRetryCron);

    recurringJobs.AddOrUpdate<DetectTimedOutProcessingJob>(
        "detect-timed-out-processing",
        job => job.ExecuteAsync(),
        options.TimedOutProcessingCron);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<TenantMiddleware>();

app.MapControllers();

app.MapHealthChecks(
    "/health/live",
    new HealthCheckOptions
    {
        Predicate = _ => false,
        ResponseWriter = HealthCheckResponseWriter.WriteAsync
    });

app.MapHealthChecks(
    "/health/ready",
    new HealthCheckOptions
    {
        Predicate = registration =>
            registration.Tags.Contains("ready"),
        ResponseWriter = HealthCheckResponseWriter.WriteAsync
    });

app.Run();

public partial class Program;
