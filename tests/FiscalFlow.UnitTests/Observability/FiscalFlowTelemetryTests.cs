using System.Diagnostics.Metrics;
using FiscalFlow.Application.Observability;

namespace FiscalFlow.UnitTests.Observability;

public sealed class FiscalFlowTelemetryTests
{
    [Fact]
    public void ProcessedCounter_ShouldPublishMeasurement()
    {
        var measurements = new List<long>();

        using var listener = new MeterListener
        {
            InstrumentPublished = (instrument, meterListener) =>
            {
                if (instrument.Meter.Name
                    == FiscalFlowTelemetry.MeterName)
                {
                    meterListener.EnableMeasurementEvents(
                        instrument);
                }
            }
        };

        listener.SetMeasurementEventCallback<long>(
            (instrument, measurement, tags, state) =>
            {
                if (instrument.Name
                    == "fiscalflow.documents.processed")
                {
                    measurements.Add(measurement);
                }
            });

        listener.Start();

        FiscalFlowTelemetry.DocumentsProcessed.Add(1);

        Assert.Contains(1, measurements);
    }
}
