using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace FiscalFlow.IntegrationTests;

public sealed class TenantMiddlewareTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TenantMiddlewareTests(
        WebApplicationFactory<Program> factory)
    {
        _client = factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration(
                    (_, configuration) =>
                    {
                        configuration.AddInMemoryCollection(
                            new Dictionary<string, string?>
                            {
                                ["MongoDb:InitializeIndexes"] = "false",
                                ["RabbitMq:Enabled"] = "false"
                            });
                    });
            })
            .CreateClient();
    }

    [Fact]
    public async Task GetDocuments_WithoutTenantHeader_ShouldReturnBadRequest()
    {
        var response = await _client.GetAsync(
            "/api/fiscal-documents");

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }
}
