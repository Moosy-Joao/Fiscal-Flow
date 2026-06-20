using FiscalFlow.Api.Configuration;
using FiscalFlow.Api.Tenancy;
using FiscalFlow.Application.Documents;
using FiscalFlow.Application.Documents.Xml;
using FiscalFlow.Infrastructure.Documents;
using FiscalFlow.Infrastructure.MongoDb;
using FiscalFlow.Infrastructure.RabbitMq;

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

var rabbitMqEnabled =
    builder.AddRabbitMqFeature();

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

if (rabbitMqEnabled)
{
    var topologyInitializer =
        app.Services.GetRequiredService<
            RabbitMqTopologyInitializer>();

    await topologyInitializer.InitializeAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<TenantMiddleware>();

app.MapControllers();

app.Run();

public partial class Program;
