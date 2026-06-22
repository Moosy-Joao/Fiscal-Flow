using System.Net;
using System.Security.Claims;
using FiscalFlow.Api.Tenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging.Abstractions;

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

    [Fact]
    public async Task AuthenticatedRequest_ShouldPreferTenantClaimOverHeader()
    {
        var context = CreateFiscalDocumentsContext();
        context.User = CreateAuthenticatedUser(
            new Claim(TenantMiddleware.TenantClaimType, "claim-tenant"));
        context.Request.Headers[TenantMiddleware.HeaderName] =
            "header-tenant";

        var tenantContext = new TenantContext();
        var nextCalled = false;
        var middleware = CreateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context, tenantContext);

        Assert.True(nextCalled);
        Assert.Equal("claim-tenant", tenantContext.TenantId);
    }

    [Fact]
    public async Task AuthenticatedRequest_WithoutTenantClaim_ShouldReturnForbidden()
    {
        var context = CreateFiscalDocumentsContext();
        context.User = CreateAuthenticatedUser();
        context.Request.Headers[TenantMiddleware.HeaderName] =
            "header-tenant";

        var tenantContext = new TenantContext();
        var nextCalled = false;
        var middleware = CreateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context, tenantContext);

        Assert.False(nextCalled);
        Assert.Equal(
            StatusCodes.Status403Forbidden,
            context.Response.StatusCode);
        Assert.Equal(string.Empty, tenantContext.TenantId);
    }

    [Fact]
    public async Task AnonymousRequest_ShouldUseTenantHeader()
    {
        var context = CreateFiscalDocumentsContext();
        context.Request.Headers[TenantMiddleware.HeaderName] =
            "header-tenant";

        var tenantContext = new TenantContext();
        var nextCalled = false;
        var middleware = CreateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context, tenantContext);

        Assert.True(nextCalled);
        Assert.Equal("header-tenant", tenantContext.TenantId);
    }

    [Theory]
    [InlineData("tenant/invalid")]
    [InlineData("tenant invalid")]
    [InlineData("tenant@invalid")]
    public void SetTenantId_WithInvalidCharacters_ShouldThrow(
        string tenantId)
    {
        var tenantContext = new TenantContext();

        Assert.Throws<ArgumentException>(
            () => tenantContext.SetTenantId(tenantId));
    }

    [Fact]
    public void SetTenantId_AboveMaximumLength_ShouldThrow()
    {
        var tenantContext = new TenantContext();
        var tenantId = new string('a', 101);

        Assert.Throws<ArgumentException>(
            () => tenantContext.SetTenantId(tenantId));
    }

    private static DefaultHttpContext CreateFiscalDocumentsContext()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/fiscal-documents";
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static ClaimsPrincipal CreateAuthenticatedUser(
        params Claim[] claims)
    {
        var identity = new ClaimsIdentity(
            claims,
            authenticationType: "Test");

        return new ClaimsPrincipal(identity);
    }

    private static TenantMiddleware CreateMiddleware(
        RequestDelegate next) =>
        new(next, NullLogger<TenantMiddleware>.Instance);
}
