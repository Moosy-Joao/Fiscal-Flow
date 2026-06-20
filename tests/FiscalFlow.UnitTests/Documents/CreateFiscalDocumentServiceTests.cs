using FiscalFlow.Application.Documents;
using FiscalFlow.Domain.Documents;
using FiscalFlow.UnitTests.Fakes;

namespace FiscalFlow.UnitTests.Documents;

public sealed class CreateFiscalDocumentServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldPersistAndPublishReceivedDocument()
    {
        var repository =
            new FakeFiscalDocumentRepository();

        var publisher =
            new FakeFiscalDocumentReceivedPublisher();

        var service =
            new CreateFiscalDocumentService(
                repository,
                publisher);

        var command = CreateCommand("NFE-002");

        var result = await service.ExecuteAsync(
            command,
            CancellationToken.None);

        var savedDocument =
            Assert.Single(repository.Documents);

        var publishedMessage =
            Assert.Single(publisher.Messages);

        Assert.Equal(
            DocumentProcessingStatus.Received,
            savedDocument.Status);

        Assert.Equal(
            "empresa-demo",
            savedDocument.TenantId);

        Assert.Equal(
            "NFE-002",
            savedDocument.ExternalDocumentId);

        Assert.Equal(savedDocument.Id, result.Id);
        Assert.Equal("Received", result.Status);
        Assert.True(result.WasCreated);

        Assert.Equal(
            savedDocument.Id,
            publishedMessage.DocumentId);

        Assert.Equal(
            savedDocument.TenantId,
            publishedMessage.TenantId);

        Assert.Equal(
            savedDocument.ExternalDocumentId,
            publishedMessage.ExternalDocumentId);

        Assert.Equal(
            ValidXml,
            savedDocument.XmlContent);

        Assert.Equal(
            savedDocument.ReceivedAtUtc,
            publishedMessage.ReceivedAtUtc);

        Assert.NotEqual(
            Guid.Empty,
            publishedMessage.CorrelationId);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRepublish_WhenRepeatedAndStillReceived()
    {
        var repository =
            new FakeFiscalDocumentRepository();

        var publisher =
            new FakeFiscalDocumentReceivedPublisher();

        var service =
            new CreateFiscalDocumentService(
                repository,
                publisher);

        var command = CreateCommand("NFE-IDEMPOTENTE");

        var firstResult =
            await service.ExecuteAsync(command);

        var secondResult =
            await service.ExecuteAsync(command);

        var savedDocument =
            Assert.Single(repository.Documents);

        Assert.True(firstResult.WasCreated);
        Assert.False(secondResult.WasCreated);

        Assert.Equal(
            firstResult.Id,
            secondResult.Id);

        Assert.Equal(
            savedDocument.Id,
            secondResult.Id);

        Assert.Equal(2, publisher.Messages.Count);
        Assert.All(
            publisher.Messages,
            message => Assert.Equal(
                firstResult.Id,
                message.DocumentId));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotRepublish_WhenDocumentIsProcessing()
    {
        var repository =
            new FakeFiscalDocumentRepository();

        var publisher =
            new FakeFiscalDocumentReceivedPublisher();

        var service =
            new CreateFiscalDocumentService(
                repository,
                publisher);

        var command = CreateCommand("NFE-PROCESSANDO");

        await service.ExecuteAsync(command);

        var savedDocument =
            Assert.Single(repository.Documents);

        savedDocument.MarkAsProcessing();

        var repeatedResult =
            await service.ExecuteAsync(command);

        Assert.False(repeatedResult.WasCreated);
        Assert.Single(publisher.Messages);
        Assert.Equal(1, publisher.PublishAttempts);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRecoverPublication_WhenInitialAttemptFails()
    {
        var repository =
            new FakeFiscalDocumentRepository();

        var publisher =
            new FakeFiscalDocumentReceivedPublisher
            {
                ExceptionToThrow =
                    new InvalidOperationException(
                        "RabbitMQ indisponível.")
            };

        var service =
            new CreateFiscalDocumentService(
                repository,
                publisher);

        var command = CreateCommand("NFE-RECUPERACAO");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExecuteAsync(command));

        var savedDocument =
            Assert.Single(repository.Documents);

        Assert.Equal(
            DocumentProcessingStatus.Received,
            savedDocument.Status);

        Assert.Empty(publisher.Messages);
        Assert.Equal(1, publisher.PublishAttempts);

        publisher.ExceptionToThrow = null;

        var repeatedResult =
            await service.ExecuteAsync(command);

        Assert.False(repeatedResult.WasCreated);
        Assert.Single(repository.Documents);
        Assert.Equal(2, publisher.PublishAttempts);

        var publishedMessage =
            Assert.Single(publisher.Messages);

        Assert.Equal(
            savedDocument.Id,
            publishedMessage.DocumentId);
    }

    private static CreateFiscalDocumentCommand CreateCommand(
        string externalDocumentId)
    {
        return new CreateFiscalDocumentCommand(
            "empresa-demo",
            externalDocumentId,
            ValidXml);
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
