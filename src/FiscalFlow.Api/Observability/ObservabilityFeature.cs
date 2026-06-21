using FiscalFlow.Application.Observability;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace FiscalFlow.Api.Observability;

internal static class ObservabilityFeature
{
    public static void AddObservabilityFeature(
        this WebApplicationBuilder builder)
    {
        var options = builder.Configuration
            .GetSection(ObservabilityOptions.SectionName)
            .Get<ObservabilityOptions>()
            ?? new ObservabilityOptions();

        if (string.IsNullOrWhiteSpace(options.ServiceName)
            || string.IsNullOrWhiteSpace(options.ServiceVersion))
        {
            throw new InvalidOperationException(
                "A seção Observability não foi configurada corretamente.");
        }

        builder.Services.AddSingleton(options);

        builder.Logging.ClearProviders();

        builder.Logging.Configure(logging =>
        {
            logging.ActivityTrackingOptions =
                ActivityTrackingOptions.TraceId
                | ActivityTrackingOptions.SpanId
                | ActivityTrackingOptions.ParentId
                | ActivityTrackingOptions.Tags
                | ActivityTrackingOptions.Baggage;
        });

        builder.Logging.AddJsonConsole(console =>
        {
            console.IncludeScopes = true;
            console.UseUtcTimestamp = true;
            console.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
        });

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
            logging.ParseStateValues = true;

            if (options.OtlpEnabled)
            {
                logging.AddOtlpExporter(exporter =>
                    ConfigureExporter(exporter, options));
            }
        });

        var openTelemetry = builder.Services
            .AddOpenTelemetry()
            .ConfigureResource(resource =>
                resource.AddService(
                    serviceName: options.ServiceName,
                    serviceVersion: options.ServiceVersion));

        openTelemetry.WithTracing(tracing =>
        {
            tracing
                .AddSource(FiscalFlowTelemetry.ActivitySourceName)
                .AddAspNetCoreInstrumentation(instrumentation =>
                {
                    instrumentation.RecordException = true;
                })
                .AddHttpClientInstrumentation(instrumentation =>
                {
                    instrumentation.RecordException = true;
                });

            if (options.OtlpEnabled)
            {
                tracing.AddOtlpExporter(exporter =>
                    ConfigureExporter(exporter, options));
            }
        });

        openTelemetry.WithMetrics(metrics =>
        {
            metrics
                .AddMeter(FiscalFlowTelemetry.MeterName)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation();

            if (options.OtlpEnabled)
            {
                metrics.AddOtlpExporter(exporter =>
                    ConfigureExporter(exporter, options));
            }
        });
    }

    private static void ConfigureExporter(
        OpenTelemetry.Exporter.OtlpExporterOptions exporter,
        ObservabilityOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.OtlpEndpoint))
        {
            return;
        }

        if (!Uri.TryCreate(
                options.OtlpEndpoint,
                UriKind.Absolute,
                out var endpoint))
        {
            throw new InvalidOperationException(
                "Observability:OtlpEndpoint deve ser uma URI absoluta.");
        }

        exporter.Endpoint = endpoint;
    }
}
