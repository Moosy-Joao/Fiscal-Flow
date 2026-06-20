using System.Text;
using System.Text.Json;
using FiscalFlow.Application.Documents;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace FiscalFlow.Infrastructure.Messaging;

public sealed class RabbitMqDispatchQueue : IDocumentDispatchQueue
{
    private readonly RabbitMqOptions _options;

    public RabbitMqDispatchQueue(IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;
    }

    public Task PublishAsync(string tenantId, string documentId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: _options.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var payload = JsonSerializer.Serialize(new { tenantId, documentId });
        var body = Encoding.UTF8.GetBytes(payload);

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;

        channel.BasicPublish(
            exchange: string.Empty,
            routingKey: _options.QueueName,
            basicProperties: properties,
            body: body);

        return Task.CompletedTask;
    }
}
