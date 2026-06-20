using FiscalFlow.Api.Middleware;
using FiscalFlow.Application.Documents;
using FiscalFlow.Infrastructure.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<TenantContext>();
builder.Services.AddHealthChecks();
builder.Services.AddFiscalFlowInfrastructure(builder.Configuration);

builder.Services
    .AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSource("FiscalFlow")
        .AddConsoleExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddMeter("FiscalFlow.Ingestion")
        .AddConsoleExporter());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<TenantResolutionMiddleware>();
app.UseHttpsRedirection();

app.MapHealthChecks("/healthz");

app.MapPost("/api/fiscal-documents", async (
    SubmitFiscalDocumentRequest request,
    HttpContext context,
    TenantContext tenant,
    IFiscalDocumentService service,
    CancellationToken cancellationToken) =>
{
    if (!context.Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKey)
        || string.IsNullOrWhiteSpace(idempotencyKey))
    {
        return Results.BadRequest(new { error = "Header Idempotency-Key é obrigatório." });
    }

    if (string.IsNullOrWhiteSpace(request.ExternalDocumentId))
    {
        return Results.BadRequest(new { error = "externalDocumentId é obrigatório." });
    }

    var result = await service.SubmitAsync(tenant.TenantId, idempotencyKey.ToString(), request, cancellationToken);

    return Results.Accepted($"/api/fiscal-documents/{result.DocumentId}", new
    {
        documentId = result.DocumentId,
        duplicate = result.IsDuplicate,
        tenantId = tenant.TenantId
    });
});

app.Run();
