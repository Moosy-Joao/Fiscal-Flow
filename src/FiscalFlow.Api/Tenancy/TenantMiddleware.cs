using Microsoft.AspNetCore.Mvc;

namespace FiscalFlow.Api.Tenancy;

public sealed class TenantMiddleware
{
    public const string HeaderName = "X-Tenant-Id";

    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        TenantContext tenantContext)
    {
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
                        StatusCodes.Status400BadRequest
                },
                context.RequestAborted);

            return;
        }

        tenantContext.SetTenantId(tenantId);

        await _next(context);
    }
}