using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FiscalFlow.IntegrationTests;

public sealed class HealthEndpointTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(
        WebApplicationFactory<Program> factory)
    {
        _client = factory
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
            })
            .CreateClient();
    }

    [Fact]
    public async Task GetHealth_ShouldReturnHealthyResponse()
    {
        var response = await _client.GetAsync("/api/health");
        var body = await response.Content
            .ReadFromJsonAsync<HealthResponse>();

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal("FiscalFlow", body.Application);
        Assert.Equal("Healthy", body.Status);
    }

    private sealed record HealthResponse(
        string Application,
        string Status,
        DateTimeOffset CheckedAtUtc);
}
