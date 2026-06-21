using Microsoft.AspNetCore.Mvc.Testing;

namespace FiscalFlow.IntegrationTests;

public sealed class RequestContextTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private const string HeaderName = "X-Correlation-ID";
    private readonly HttpClient _client;

    public RequestContextTests(
        WebApplicationFactory<Program> factory)
    {
        _client = factory
            .WithWebHostBuilder(
                TenantRabbitMqConfiguration.UseTestingEnvironment)
            .CreateClient();
    }

    [Fact]
    public async Task RequestWithoutId_ShouldReturnGeneratedHeader()
    {
        var response = await _client.GetAsync("/api/health");

        var value = Assert.Single(
            response.Headers.GetValues(HeaderName));

        Assert.False(string.IsNullOrWhiteSpace(value));
    }

    [Fact]
    public async Task RequestWithId_ShouldPreserveValue()
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/health");

        request.Headers.Add(HeaderName, "integration-test-id");

        var response = await _client.SendAsync(request);

        Assert.Equal(
            "integration-test-id",
            Assert.Single(response.Headers.GetValues(HeaderName)));
    }
}
