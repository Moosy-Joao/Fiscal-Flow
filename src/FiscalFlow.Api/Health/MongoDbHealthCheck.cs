using FiscalFlow.Infrastructure.MongoDb;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FiscalFlow.Api.Health;

public sealed class MongoDbHealthCheck : IHealthCheck
{
    private readonly MongoDbContext _mongoDbContext;

    public MongoDbHealthCheck(MongoDbContext mongoDbContext)
    {
        ArgumentNullException.ThrowIfNull(mongoDbContext);
        _mongoDbContext = mongoDbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _mongoDbContext.PingAsync(cancellationToken);
            return HealthCheckResult.Healthy("MongoDB disponível.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(
                "MongoDB indisponível.",
                exception);
        }
    }
}
