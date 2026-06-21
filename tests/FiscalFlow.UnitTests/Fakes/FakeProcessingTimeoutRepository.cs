using FiscalFlow.Application.Documents;

namespace FiscalFlow.UnitTests.Fakes;

internal sealed class FakeProcessingTimeoutRepository
    : IProcessingTimeoutRepository
{
    public DateTimeOffset? StartedBeforeUtc { get; private set; }

    public int? BatchSize { get; private set; }

    public int Result { get; set; }

    public Task<int> MarkTimedOutProcessingAsFailedAsync(
        DateTimeOffset startedBeforeUtc,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        StartedBeforeUtc = startedBeforeUtc;
        BatchSize = batchSize;

        return Task.FromResult(Result);
    }
}
