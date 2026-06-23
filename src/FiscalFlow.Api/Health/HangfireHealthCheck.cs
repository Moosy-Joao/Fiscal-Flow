using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FiscalFlow.Api.Health;

public sealed class HangfireHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = JobStorage.Current.GetConnection();
            connection.GetRecurringJobs();

            return Task.FromResult(
                HealthCheckResult.Healthy("Hangfire storage reachable."));
        }
        catch (Exception exception)
        {
            return Task.FromResult(
                HealthCheckResult.Unhealthy(
                    "Hangfire storage unavailable.",
                    exception));
        }
    }
}
