using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FiscalFlow.IntegrationTests;

public sealed class TenantMiddlewareTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TenantMiddlewareTests(
        WebApplicationFactory<Program> factory)
    {
        _client = factory
            .WithWebHostBuilder(
                TenantRabbitMqConfiguration
                    .UseTestingEnvironment)
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
