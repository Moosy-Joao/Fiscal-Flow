using FiscalFlow.Api.Tenancy;
using FiscalFlow.Application.Documents;
using FiscalFlow.Infrastructure.Documents;
using FiscalFlow.Infrastructure.MongoDb;
using FiscalFlow.Infrastructure.RabbitMq;
using FiscalFlow.Application.Messaging;
using FiscalFlow.Api.Messaging;
using FiscalFlow.Application.Documents.Xml;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

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

var rabbitMqOptions = builder.Configuration
    .GetSection(RabbitMqOptions.SectionName)
    .Get<RabbitMqOptions>();

if (rabbitMqOptions is null
    || string.IsNullOrWhiteSpace(
        rabbitMqOptions.HostName)
    || string.IsNullOrWhiteSpace(
        rabbitMqOptions.UserName)
    || string.IsNullOrWhiteSpace(
        rabbitMqOptions.Password)
    || string.IsNullOrWhiteSpace(
        rabbitMqOptions.VirtualHost)
    || string.IsNullOrWhiteSpace(
        rabbitMqOptions.ExchangeName)
    || string.IsNullOrWhiteSpace(
        rabbitMqOptions.QueueName)
    || string.IsNullOrWhiteSpace(
        rabbitMqOptions.RoutingKey)
    || rabbitMqOptions.Port <= 0)
{
    throw new InvalidOperationException(
        "A seção RabbitMq não foi configurada corretamente.");
}

builder.Services.AddSingleton(mongoDbOptions);
builder.Services.AddSingleton<MongoDbContext>();

builder.Services.AddSingleton<
    FiscalDocumentIndexManager>();

builder.Services.AddSingleton<
    IFiscalDocumentRepository,
    FiscalDocumentRepository>();

builder.Services.AddSingleton<
    IFiscalDocumentXmlParser,
    FiscalDocumentXmlParser>();

builder.Services.AddSingleton(rabbitMqOptions);

builder.Services.AddSingleton<
    RabbitMqConnectionFactory>();

builder.Services.AddSingleton<
    RabbitMqTopologyInitializer>();

builder.Services.AddSingleton<
    IFiscalDocumentReceivedPublisher,
    RabbitMqFiscalDocumentReceivedPublisher>();

builder.Services.AddHostedService<
    FiscalDocumentReceivedConsumer>();

builder.Services.AddScoped<
    CreateFiscalDocumentService>();

builder.Services.AddScoped<
    ProcessFiscalDocumentService>();

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

var rabbitMqTopologyInitializer =
    app.Services.GetRequiredService<
        RabbitMqTopologyInitializer>();

await rabbitMqTopologyInitializer.InitializeAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<TenantMiddleware>();

app.MapControllers();

app.Run();

public partial class Program;