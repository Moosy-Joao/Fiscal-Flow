using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FiscalFlow.Api.Health;

public static class HealthCheckResponseWriter
{
    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web)
        {
            WriteIndented = true
        };

    public static Task WriteAsync(
        HttpContext context,
        HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.ToDictionary(
                entry => entry.Key,
                entry => new
                {
                    status = entry.Value.Status.ToString(),
                    durationMs = entry.Value.Duration.TotalMilliseconds,
                    description = entry.Value.Description
                })
        };

        return context.Response.WriteAsync(
            JsonSerializer.Serialize(
                response,
                SerializerOptions));
    }
}
