using System.Net;
using System.Net.Http.Json;
using System.Text;
using FiscalFlow.Application.Documents;

namespace FiscalFlow.IntegrationTests;

public sealed class FiscalDocumentUploadEndpointTests :
    IClassFixture<FiscalFlowApiFactory>
{
    private const string TenantId = "empresa-upload";
    private const int MaximumFileSizeBytes =
        2 * 1024 * 1024;

    private readonly FiscalFlowApiFactory _factory;
    private readonly HttpClient _client;

    public FiscalDocumentUploadEndpointTests(
        FiscalFlowApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UploadValidXml_ShouldCreateDocumentAndRemainIdempotent()
    {
        var externalDocumentId =
            $"NFE-UPLOAD-{Guid.NewGuid():N}";

        using var firstRequest = CreateUploadRequest(
            externalDocumentId,
            "nota-fiscal.xml",
            Encoding.UTF8.GetBytes(ValidXml),
            includeTenant: true);

        var firstResponse = await _client.SendAsync(
            firstRequest);

        Assert.Equal(
            HttpStatusCode.Created,
            firstResponse.StatusCode);

        var firstResult =
            await firstResponse.Content
                .ReadFromJsonAsync<CreateFiscalDocumentResult>();

        Assert.NotNull(firstResult);
        Assert.True(firstResult.WasCreated);
        Assert.Equal(TenantId, firstResult.TenantId);
        Assert.Equal(
            externalDocumentId,
            firstResult.ExternalDocumentId);
        Assert.Equal("Received", firstResult.Status);

        using var repeatedRequest = CreateUploadRequest(
            externalDocumentId,
            "nota-fiscal.xml",
            Encoding.UTF8.GetBytes(ValidXml),
            includeTenant: true);

        var repeatedResponse = await _client.SendAsync(
            repeatedRequest);

        Assert.Equal(
            HttpStatusCode.OK,
            repeatedResponse.StatusCode);

        var repeatedResult =
            await repeatedResponse.Content
                .ReadFromJsonAsync<CreateFiscalDocumentResult>();

        Assert.NotNull(repeatedResult);
        Assert.False(repeatedResult.WasCreated);
        Assert.Equal(firstResult.Id, repeatedResult.Id);

        var savedDocuments = _factory.Repository.Documents
            .Where(document =>
                document.TenantId == TenantId
                && document.ExternalDocumentId
                    == externalDocumentId)
            .ToList();

        var savedDocument = Assert.Single(savedDocuments);
        Assert.Equal(firstResult.Id, savedDocument.Id);
        Assert.Contains("infNFe", savedDocument.XmlContent);
    }

    [Fact]
    public async Task UploadWithoutTenant_ShouldReturnBadRequest()
    {
        using var request = CreateUploadRequest(
            $"NFE-SEM-TENANT-{Guid.NewGuid():N}",
            "nota-fiscal.xml",
            Encoding.UTF8.GetBytes(ValidXml),
            includeTenant: false);

        var response = await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task UploadWithInvalidExtension_ShouldReturnBadRequest()
    {
        using var request = CreateUploadRequest(
            $"NFE-EXTENSAO-{Guid.NewGuid():N}",
            "nota-fiscal.txt",
            Encoding.UTF8.GetBytes(ValidXml),
            includeTenant: true);

        var response = await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task UploadWithMalformedXml_ShouldReturnBadRequest()
    {
        using var request = CreateUploadRequest(
            $"NFE-INVALIDA-{Guid.NewGuid():N}",
            "nota-fiscal.xml",
            Encoding.UTF8.GetBytes("<nfeProc>"),
            includeTenant: true);

        var response = await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task UploadAboveLimit_ShouldReturnPayloadTooLarge()
    {
        var oversizedContent = new byte[
            MaximumFileSizeBytes + 1];

        Array.Fill(oversizedContent, (byte)'x');

        using var request = CreateUploadRequest(
            $"NFE-GRANDE-{Guid.NewGuid():N}",
            "nota-fiscal.xml",
            oversizedContent,
            includeTenant: true);

        var response = await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.RequestEntityTooLarge,
            response.StatusCode);
    }

    private static HttpRequestMessage CreateUploadRequest(
        string externalDocumentId,
        string fileName,
        byte[] fileContent,
        bool includeTenant)
    {
        var multipart = new MultipartFormDataContent();
        multipart.Add(
            new StringContent(externalDocumentId),
            "externalDocumentId");

        var file = new ByteArrayContent(fileContent);
        file.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue(
                "application/xml");

        multipart.Add(file, "file", fileName);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/fiscal-documents/upload")
        {
            Content = multipart
        };

        if (includeTenant)
        {
            request.Headers.Add(
                "X-Tenant-Id",
                TenantId);
        }

        return request;
    }

    private const string ValidXml =
        """
        <nfeProc xmlns="http://www.portalfiscal.inf.br/nfe">
          <NFe>
            <infNFe Id="NFe41260612345678000195550010000012341000012345">
              <ide>
                <dhEmi>2026-06-20T10:30:00-03:00</dhEmi>
              </ide>
              <emit>
                <CNPJ>12345678000195</CNPJ>
                <xNome>Empresa Emitente Ltda</xNome>
              </emit>
              <dest>
                <CPF>12345678901</CPF>
                <xNome>Cliente Destinatário</xNome>
              </dest>
              <total>
                <ICMSTot>
                  <vNF>1500.75</vNF>
                </ICMSTot>
              </total>
            </infNFe>
          </NFe>
        </nfeProc>
        """;
}
