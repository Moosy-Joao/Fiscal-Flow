using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FiscalFlow.Api.Tenancy;

public sealed class TenantMiddleware
{
    public const string HeaderName = "X-Tenant-Id";
    public const string CorrelationHeaderName = "X-Correlation-ID";
    public const string TenantClaimType = "tenant_id";

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

        Activity.Current?.SetTag(
            "correlation.id",
            correlationId);

        Activity.Current?.AddBaggage(
            "correlation.id",
            correlationId);

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

        var tenantId = ResolveTenantId(context);

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            await WriteTenantProblemAsync(
                context,
                correlationId);

            return;
        }

        tenantContext.SetTenantId(tenantId);

        Activity.Current?.SetTag(
            "tenant.id",
            tenantContext.TenantId);

        using var tenantScope = _logger.BeginScope(
            new Dictionary<string, object>
            {
                ["TenantId"] = tenantContext.TenantId
            });

        await _next(context);
    }

    private static string? ResolveTenantId(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            return context.User.FindFirst(TenantClaimType)?.Value;
        }

        return context.Request.Headers[HeaderName]
            .FirstOrDefault();
    }

    private static Task WriteTenantProblemAsync(
        HttpContext context,
        string correlationId)
    {
        var authenticated =
            context.User.Identity?.IsAuthenticated == true;

        var statusCode = authenticated
            ? StatusCodes.Status403Forbidden
            : StatusCodes.Status400BadRequest;

        context.Response.StatusCode = statusCode;

        return context.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Title = authenticated
                    ? "Tenant ausente na identidade"
                    : "Tenant não informado",
                Detail = authenticated
                    ? $"O token autenticado deve possuir a claim '{TenantClaimType}'."
                    : $"Informe o cabeçalho '{HeaderName}'.",
                Status = statusCode,
                Extensions =
                {
                    ["correlationId"] = correlationId
                }
            },
            context.RequestAborted);
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
