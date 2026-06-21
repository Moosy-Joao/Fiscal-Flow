using Microsoft.AspNetCore.Mvc;

namespace FiscalFlow.Api.Tenancy;

public sealed class TenantMiddleware
{
    public const string HeaderName = "X-Tenant-Id";
    public const string CorrelationHeaderName = "X-Correlation-ID";

    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;

    public TenantMiddleware(
        RequestDelegate next,
        ILogger<TenantMiddleware> logger)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(logger);

        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        TenantContext tenantContext)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(tenantContext);

        var correlationId = ResolveCorrelationId(context);

        context.TraceIdentifier = correlationId;
        context.Response.Headers[CorrelationHeaderName] =
            correlationId;

        using var scope = _logger.BeginScope(
            new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId
            });

        var requiresTenant =
            context.Request.Path.StartsWithSegments(
                "/api/fiscal-documents");

        if (!requiresTenant)
        {
            await _next(context);
            return;
        }

        var tenantId =
            context.Request.Headers[HeaderName]
                .ToString();

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            context.Response.StatusCode =
                StatusCodes.Status400BadRequest;

            await context.Response.WriteAsJsonAsync(
                new ProblemDetails
                {
                    Title = "Tenant não informado",
                    Detail =
                        $"Informe o cabeçalho '{HeaderName}'.",
                    Status =
                        StatusCodes.Status400BadRequest,
                    Extensions =
                    {
                        ["correlationId"] = correlationId
                    }
                },
                context.RequestAborted);

            return;
        }

        tenantContext.SetTenantId(tenantId);

        using var tenantScope = _logger.BeginScope(
            new Dictionary<string, object>
            {
                ["TenantId"] = tenantContext.TenantId
            });

        await _next(context);
    }

    private static string ResolveCorrelationId(
        HttpContext context)
    {
        var received = context.Request
            .Headers[CorrelationHeaderName]
            .FirstOrDefault()
            ?.Trim();

        if (!string.IsNullOrWhiteSpace(received)
            && received.Length <= 128)
        {
            return received;
        }

        return Guid.NewGuid().ToString("N");
    }
}
