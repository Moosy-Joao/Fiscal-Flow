using Microsoft.AspNetCore.Hosting;

namespace FiscalFlow.IntegrationTests;

internal static class TenantRabbitMqConfiguration
{
    public static void UseTestingEnvironment(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment(
            IntegrationTestSettings.EnvironmentName);
    }
}
