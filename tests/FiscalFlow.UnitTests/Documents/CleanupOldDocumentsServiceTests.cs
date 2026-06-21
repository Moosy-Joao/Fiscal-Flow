using FiscalFlow.Application.Documents;
using FiscalFlow.UnitTests.Fakes;

namespace FiscalFlow.UnitTests.Documents;

public sealed class CleanupOldDocumentsServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldCalculateRetentionCutoff()
    {
        var repository = new FakeDocumentCleanupRepository
        {
            Result = 4
        };

        var service = new CleanupOldDocumentsService(repository);
        var utcNow = new DateTimeOffset(
            2026,
            6,
            21,
            12,
            0,
            0,
            TimeSpan.Zero);

        var result = await service.ExecuteAsync(
            new CleanupOldDocumentsCommand(
                RetentionDays: 90,
                BatchSize: 100,
                utcNow));

        Assert.Equal(4, result);
        Assert.Equal(
            utcNow.AddDays(-90),
            repository.ReceivedBeforeUtc);
        Assert.Equal(100, repository.BatchSize);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRejectInvalidRetention()
    {
        var service = new CleanupOldDocumentsService(
            new FakeDocumentCleanupRepository());

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.ExecuteAsync(
                new CleanupOldDocumentsCommand(
                    RetentionDays: 0,
                    BatchSize: 100,
                    DateTimeOffset.UtcNow)));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRejectInvalidBatchSize()
    {
        var service = new CleanupOldDocumentsService(
            new FakeDocumentCleanupRepository());

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.ExecuteAsync(
                new CleanupOldDocumentsCommand(
                    RetentionDays: 90,
                    BatchSize: 0,
                    DateTimeOffset.UtcNow)));
    }
}
