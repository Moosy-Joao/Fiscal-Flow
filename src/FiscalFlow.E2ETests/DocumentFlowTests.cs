using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace FiscalFlow.E2ETests;

public class DocumentFlowTests :
    IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public DocumentFlowTests(
        WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Should_Create_And_Get_Document()
    {
        // Arrange

        var request = new
        {
            externalDocumentId =
                $"TEST-{Guid.NewGuid()}",

            xmlContent =
                """
                <NFe>
                    <ide>
                        <cNF>123</cNF>
                    </ide>
                </NFe>
                """
        };

        _client.DefaultRequestHeaders.Add(
            "X-Tenant-Id",
            "tenant-e2e");

        // Act

        var createResponse =
            await _client.PostAsJsonAsync(
                "/api/fiscal-documents",
                request);

        // Assert Create

        createResponse.StatusCode.Should()
            .Be(HttpStatusCode.Created);

        var created =
            await createResponse.Content
                .ReadFromJsonAsync<CreateResponse>();

        created.Should().NotBeNull();

        created!.Id.Should().NotBe(Guid.Empty);

        created.TenantId.Should()
            .Be("tenant-e2e");

        created.ExternalDocumentId.Should()
            .StartWith("TEST-");

        created.WasCreated.Should()
            .BeTrue();

        // Act Get

        var getResponse =
            await _client.GetAsync(
                $"/api/fiscal-documents/{created!.Id}");

        // Assert Get

        getResponse.StatusCode.Should()
    .Be(HttpStatusCode.OK);

        var document =
            await getResponse.Content
                .ReadFromJsonAsync<GetDocumentResponse>();

        document.Should().NotBeNull();

        document!.Id.Should()
            .Be(created.Id);

        document.TenantId.Should()
            .Be("tenant-e2e");

        document.ExternalDocumentId.Should()
            .Be(created.ExternalDocumentId);
    }

    private sealed class CreateResponse
    {
        public Guid Id { get; set; }

        public string TenantId { get; set; } = string.Empty;

        public string ExternalDocumentId { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public bool WasCreated { get; set; }
    }

    private sealed class GetDocumentResponse
    {
        public Guid Id { get; set; }

        public string TenantId { get; set; } = string.Empty;

        public string ExternalDocumentId { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
    }
}