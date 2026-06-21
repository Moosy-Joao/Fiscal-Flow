using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace FiscalFlow.Application.Observability;

public static class FiscalFlowTelemetry
{
    public const string ActivitySourceName = "FiscalFlow.Documents";
    public const string MeterName = "FiscalFlow.Documents";

    public static readonly ActivitySource ActivitySource =
        new(ActivitySourceName);

    public static readonly Meter Meter =
        new(MeterName);

    public static readonly Counter<long> DocumentsReceived =
        Meter.CreateCounter<long>("fiscalflow.documents.received");

    public static readonly Counter<long> DocumentsProcessed =
        Meter.CreateCounter<long>("fiscalflow.documents.processed");

    public static readonly Counter<long> DocumentsFailed =
        Meter.CreateCounter<long>("fiscalflow.documents.failed");

    public static readonly Counter<long> DocumentsDeadLettered =
        Meter.CreateCounter<long>("fiscalflow.documents.dead_lettered");

    public static readonly Histogram<double> ProcessingDuration =
        Meter.CreateHistogram<double>(
            "fiscalflow.documents.processing.duration",
            unit: "ms");
}
