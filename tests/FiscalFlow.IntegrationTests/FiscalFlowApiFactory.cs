using FiscalFlow.Application.Documents;
using FiscalFlow.IntegrationTests.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FiscalFlow.IntegrationTests;

public sealed class FiscalFlowApiFactory :
    WebApplicationFactory<Program>
{
    public InMemoryFiscalDocumentRepository Repository
    {
        get;
    } = new();

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment(
            IntegrationTestSettings.EnvironmentName);

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IFiscalDocumentRepository>();
            services.AddSingleton<IFiscalDocumentRepository>(
                Repository);
        });
    }
}
