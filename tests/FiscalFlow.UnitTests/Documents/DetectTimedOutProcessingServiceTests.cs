using FiscalFlow.Application.Documents;
using FiscalFlow.UnitTests.Fakes;

namespace FiscalFlow.UnitTests.Documents;

public sealed class DetectTimedOutProcessingServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldCalculateCutoffAndReturnCount()
    {
        var repository = new FakeProcessingTimeoutRepository
        {
            Result = 2
        };

        var service = new DetectTimedOutProcessingService(repository);
        var utcNow = new DateTimeOffset(
            2026,
            6,
            20,
            12,
            0,
            0,
            TimeSpan.Zero);

        var result = await service.ExecuteAsync(
            new DetectTimedOutProcessingCommand(
                TimeSpan.FromMinutes(15),
                BatchSize: 20,
                utcNow));

        Assert.Equal(2, result);
        Assert.Equal(
            utcNow.AddMinutes(-15),
            repository.StartedBeforeUtc);
        Assert.Equal(20, repository.BatchSize);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRejectInvalidTimeout()
    {
        var service = new DetectTimedOutProcessingService(
            new FakeProcessingTimeoutRepository());

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.ExecuteAsync(
                new DetectTimedOutProcessingCommand(
                    TimeSpan.Zero,
                    BatchSize: 20,
                    DateTimeOffset.UtcNow)));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRejectInvalidBatchSize()
    {
        var service = new DetectTimedOutProcessingService(
            new FakeProcessingTimeoutRepository());

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.ExecuteAsync(
                new DetectTimedOutProcessingCommand(
                    TimeSpan.FromMinutes(15),
                    BatchSize: 0,
                    DateTimeOffset.UtcNow)));
    }
}
