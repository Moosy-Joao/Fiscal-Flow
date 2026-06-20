using FiscalFlow.Api.Messaging;
using FiscalFlow.Application.Messaging;
using FiscalFlow.Infrastructure.RabbitMq;

namespace FiscalFlow.Api.Configuration;

internal static class RabbitMqFeature
{
    public static bool AddRabbitMqFeature(
        this WebApplicationBuilder builder)
    {
        var enabled = builder.Configuration.GetValue(
            "RabbitMq:Enabled",
            true);

        if (!enabled)
        {
            builder.Services.AddSingleton<
                IFiscalDocumentReceivedPublisher,
                DisabledFiscalDocumentReceivedPublisher>();

            return false;
        }

        var options = builder.Configuration
            .GetSection(RabbitMqOptions.SectionName)
            .Get<RabbitMqOptions>();

        if (options is null
            || string.IsNullOrWhiteSpace(options.HostName)
            || string.IsNullOrWhiteSpace(options.UserName)
            || string.IsNullOrWhiteSpace(options.Password)
            || string.IsNullOrWhiteSpace(options.VirtualHost)
            || string.IsNullOrWhiteSpace(options.ExchangeName)
            || string.IsNullOrWhiteSpace(options.QueueName)
            || string.IsNullOrWhiteSpace(options.RoutingKey)
            || options.Port <= 0)
        {
            throw new InvalidOperationException(
                "A seção RabbitMq não foi configurada corretamente.");
        }

        builder.Services.AddSingleton(options);
        builder.Services.AddSingleton<RabbitMqConnectionFactory>();
        builder.Services.AddSingleton<RabbitMqTopologyInitializer>();
        builder.Services.AddSingleton<
            IFiscalDocumentReceivedPublisher,
            RabbitMqFiscalDocumentReceivedPublisher>();
        builder.Services.AddHostedService<
            FiscalDocumentReceivedConsumer>();

        return true;
    }
}
