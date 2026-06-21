using FiscalFlow.Infrastructure.RabbitMq;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FiscalFlow.Api.Health;

public sealed class RabbitMqHealthCheck : IHealthCheck
{
    private readonly RabbitMqConnectionFactory _connectionFactory;

    public RabbitMqHealthCheck(
        RabbitMqConnectionFactory connectionFactory)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        _connectionFactory = connectionFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connection =
                await _connectionFactory.GetConnectionAsync(
                    cancellationToken);

            return connection.IsOpen
                ? HealthCheckResult.Healthy(
                    "RabbitMQ disponível.")
                : HealthCheckResult.Unhealthy(
                    "Conexão RabbitMQ fechada.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(
                "RabbitMQ indisponível.",
                exception);
        }
    }
}
