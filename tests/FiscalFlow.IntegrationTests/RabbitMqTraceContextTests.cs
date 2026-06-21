using System.Diagnostics;
using FiscalFlow.Infrastructure.RabbitMq;
using RabbitMQ.Client;

namespace FiscalFlow.IntegrationTests;

public sealed class RabbitMqTraceContextTests
{
    [Fact]
    public void InjectAndExtract_ShouldPreserveParentAndCorrelationId()
    {
        using var activity = new Activity("publisher")
            .SetIdFormat(ActivityIdFormat.W3C)
            .Start();

        var properties = new BasicProperties();

        RabbitMqTraceContext.Inject(
            properties,
            activity,
            "correlation-test");

        var extracted =
            RabbitMqTraceContext.Extract(properties);

        Assert.True(extracted.HasParent);
        Assert.Equal(
            activity.TraceId,
            extracted.ParentContext.TraceId);
        Assert.Equal(
            activity.SpanId,
            extracted.ParentContext.SpanId);
        Assert.Equal(
            "correlation-test",
            extracted.CorrelationId);
    }

    [Fact]
    public void ExtractWithoutTraceHeaders_ShouldUsePropertyCorrelationId()
    {
        var properties = new BasicProperties
        {
            CorrelationId = "fallback-correlation"
        };

        var extracted =
            RabbitMqTraceContext.Extract(properties);

        Assert.False(extracted.HasParent);
        Assert.Equal(
            "fallback-correlation",
            extracted.CorrelationId);
    }
}
