using System.Diagnostics;
using System.Text;
using RabbitMQ.Client;

namespace FiscalFlow.Infrastructure.RabbitMq;

public static class RabbitMqTraceContext
{
    private const string TraceParentHeader = "traceparent";
    private const string TraceStateHeader = "tracestate";
    private const string CorrelationHeader = "x-correlation-id";

    public static void Inject(
        BasicProperties properties,
        Activity? activity,
        string correlationId)
    {
        ArgumentNullException.ThrowIfNull(properties);
        ArgumentException.ThrowIfNullOrWhiteSpace(correlationId);

        properties.Headers ??=
            new Dictionary<string, object?>();

        properties.Headers[CorrelationHeader] =
            Encoding.UTF8.GetBytes(correlationId);

        if (activity?.Id is not null)
        {
            properties.Headers[TraceParentHeader] =
                Encoding.UTF8.GetBytes(activity.Id);
        }

        if (!string.IsNullOrWhiteSpace(
                activity?.TraceStateString))
        {
            properties.Headers[TraceStateHeader] =
                Encoding.UTF8.GetBytes(
                    activity.TraceStateString);
        }
    }

    public static RabbitMqExtractedTraceContext Extract(
        IReadOnlyBasicProperties properties)
    {
        ArgumentNullException.ThrowIfNull(properties);

        var traceParent = ReadHeader(
            properties.Headers,
            TraceParentHeader);

        var traceState = ReadHeader(
            properties.Headers,
            TraceStateHeader);

        var correlationId =
            ReadHeader(
                properties.Headers,
                CorrelationHeader)
            ?? properties.CorrelationId;

        var hasParent = ActivityContext.TryParse(
            traceParent,
            traceState,
            out var parentContext);

        return new RabbitMqExtractedTraceContext(
            hasParent,
            parentContext,
            correlationId);
    }

    private static string? ReadHeader(
        IDictionary<string, object?>? headers,
        string name)
    {
        if (headers is null
            || !headers.TryGetValue(name, out var value))
        {
            return null;
        }

        return value switch
        {
            string text => text,
            byte[] bytes => Encoding.UTF8.GetString(bytes),
            ReadOnlyMemory<byte> memory =>
                Encoding.UTF8.GetString(memory.Span),
            ArraySegment<byte> segment =>
                Encoding.UTF8.GetString(
                    segment.Array!,
                    segment.Offset,
                    segment.Count),
            _ => value?.ToString()
        };
    }
}

public sealed record RabbitMqExtractedTraceContext(
    bool HasParent,
    ActivityContext ParentContext,
    string? CorrelationId);
