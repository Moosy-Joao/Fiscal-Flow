using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using FiscalFlow.Application.Documents;
using FiscalFlow.IntegrationTests.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace FiscalFlow.IntegrationTests;

public sealed class SecurityEndpointTests :
    IClassFixture<SecurityApiFactory>
{
    private readonly HttpClient _client;

    public SecurityEndpointTests(
        SecurityApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task FiscalEndpoint_WithoutToken_ShouldReturnUnauthorized()
    {
        var response = await _client.GetAsync(
            "/api/fiscal-documents");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);

        var problem = await response.Content
            .ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(
            StatusCodes.Status401Unauthorized,
            problem.Status);
    }

    [Fact]
    public async Task TokenWithoutTenant_ShouldReturnForbidden()
    {
        var token = CreateToken(
            subject: "user-without-tenant",
            tenantId: null);

        var response = await GetFiscalDocumentsAsync(token);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task ValidToken_ShouldAllowFiscalEndpoint()
    {
        var token = CreateToken(
            subject: "valid-user",
            tenantId: "tenant-security");

        var response = await GetFiscalDocumentsAsync(token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HealthEndpoint_WithoutToken_ShouldRemainPublic()
    {
        var response = await _client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task FiscalEndpoint_AboveLimit_ShouldReturnTooManyRequests()
    {
        var token = CreateToken(
            subject: "rate-limit-user",
            tenantId: "tenant-rate-limit");

        var first = await GetFiscalDocumentsAsync(token);
        var second = await GetFiscalDocumentsAsync(token);
        var third = await GetFiscalDocumentsAsync(token);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.Equal(
            HttpStatusCode.TooManyRequests,
            third.StatusCode);
    }

    private async Task<HttpResponseMessage>
        GetFiscalDocumentsAsync(string token)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/fiscal-documents");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        return await _client.SendAsync(request);
    }

    private static string CreateToken(
        string subject,
        string? tenantId)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, subject)
        };

        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            claims.Add(new Claim("tenant_id", tenantId));
        }

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    SecurityApiFactory.SigningKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: SecurityApiFactory.Issuer,
            audience: SecurityApiFactory.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow.AddMinutes(-1),
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}

public sealed class SecurityApiFactory :
    WebApplicationFactory<Program>
{
    public const string EnvironmentName =
        "SecurityTesting";

    public const string Issuer = "FiscalFlow.Tests";
    public const string Audience = "FiscalFlow.Api";
    public const string SigningKey =
        "fiscal-flow-tests-signing-key-2026-secure";

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment(EnvironmentName);

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IFiscalDocumentRepository>();
            services.AddSingleton<IFiscalDocumentRepository>(
                new InMemoryFiscalDocumentRepository());
        });
    }
}
