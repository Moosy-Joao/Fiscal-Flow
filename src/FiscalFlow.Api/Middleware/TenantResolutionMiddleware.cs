namespace FiscalFlow.Api.Middleware;

public sealed class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, TenantContext tenantContext)
    {
        if (!context.Request.Headers.TryGetValue(TenantContext.HeaderName, out var tenantHeader)
            || string.IsNullOrWhiteSpace(tenantHeader))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = $"Header {TenantContext.HeaderName} é obrigatório." });
            return;
        }

        tenantContext.TenantId = tenantHeader.ToString();
        await _next(context);
    }
}
