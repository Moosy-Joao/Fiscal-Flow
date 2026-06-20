using FiscalFlow.Application.Documents;
using FiscalFlow.Infrastructure.Documents;
using FiscalFlow.Infrastructure.MongoDb;

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

builder.Services.AddScoped<
    CreateFiscalDocumentService>();

builder.Services.AddScoped<
    GetFiscalDocumentByIdService>();

builder.Services.AddScoped<
    UpdateFiscalDocumentStatusService>();

builder.Services.AddScoped<
    ListFiscalDocumentsService>();

var app = builder.Build();

if (mongoDbOptions.InitializeIndexes)
{
    var indexManager =
        app.Services.GetRequiredService<
            FiscalDocumentIndexManager>();

    await indexManager.EnsureCreatedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.Run();

public partial class Program;