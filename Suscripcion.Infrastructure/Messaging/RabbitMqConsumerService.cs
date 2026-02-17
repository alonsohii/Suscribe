using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Suscripcion.Application.Messages;
using Suscripcion.Infrastructure.Consumers;

namespace Suscripcion.Infrastructure.Messaging;

/// <summary>
/// Servicio en segundo plano que consume mensajes de RabbitMQ.
/// Procesa suscripciones y notificaciones webhook de forma asíncrona.
/// </summary>
public class RabbitMqConsumerService : BackgroundService
{
    private readonly IConnectionFactory _factory;
    private readonly IServiceProvider _services;
    private readonly IConfiguration _config;
    private readonly string _queueName;
    private IConnection? _connection;
    private IChannel? _channel;

    /// <summary>
    /// Inicializa el servicio consumidor de RabbitMQ.
    /// </summary>
    public RabbitMqConsumerService(IConnectionFactory factory, IServiceProvider services, IConfiguration config)
    {
        _factory = factory;
        _services = services;
        _config = config;
        _queueName = config.GetSection("RabbitMq")["QueueName"] ?? "subscription-queue";
    }

    /// <summary>
    /// Ejecuta el servicio: establece conexión, configura colas e inicia el consumo de mensajes.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await ConnectToRabbitMqAsync(stoppingToken);
            await SetupQueuesAsync();
            await StartConsumingAsync();
            await Task.Delay(Timeout.Infinite, stoppingToken); // Mantiene el servicio activo hasta que se cancele
        }
        catch (Exception ex)
        {
            // RabbitMQ no disponible, el servicio continúa sin mensajería
        }
    }

    /// <summary>
    /// Establece conexión con RabbitMQ usando política de reintentos (5 intentos, 3s de espera).
    /// </summary>
    private async Task ConnectToRabbitMqAsync(CancellationToken stoppingToken)
    {
        var retryCount = _config.GetValue<int>("RabbitMq:RetryCount", 5);
        var retryDelay = _config.GetValue<int>("RabbitMq:RetryDelaySeconds", 3);
        
        var retryPipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = retryCount,
                Delay = TimeSpan.FromSeconds(retryDelay)
            })
            .Build();

        await retryPipeline.ExecuteAsync(async ct =>
        {
            _connection = await _factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
        }, stoppingToken);
    }

    /// <summary>
    /// Declara las colas necesarias: suscripciones, webhooks y cola de errores.
    /// Configura Dead Letter Queue para mensajes fallidos.
    /// </summary>
    private async Task SetupQueuesAsync()
    {
        // Cola de suscripciones
        await _channel!.QueueDeclareAsync(_queueName, durable: true, exclusive: false, autoDelete: false);
        
        // Cola de errores para webhooks
        await _channel.QueueDeclareAsync("webhook-notification-queue_error", durable: true, exclusive: false, autoDelete: false);
        
        // Cola de webhooks con Dead Letter Exchange configurado
        var args = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", "" },
            { "x-dead-letter-routing-key", "webhook-notification-queue_error" }
        };
        
        await _channel.QueueDeclareAsync("webhook-notification-queue", 
            durable: true, exclusive: false, autoDelete: false, arguments: args);
    }

    /// <summary>
    /// Inicia el consumo de mensajes de las colas configuradas.
    /// </summary>
    private async Task StartConsumingAsync()
    {
        var consumer = new AsyncEventingBasicConsumer(_channel!);
        consumer.ReceivedAsync += ProcessMessageAsync;
        await _channel!.BasicConsumeAsync(_queueName, false, consumer);
        await _channel.BasicConsumeAsync("webhook-notification-queue", false, consumer);
    }

    /// <summary>
    /// Procesa un mensaje recibido. Si tiene éxito, confirma (ACK); si falla, rechaza (NACK).
    /// </summary>
    private async Task ProcessMessageAsync(object model, BasicDeliverEventArgs ea)
    {
        var json = Encoding.UTF8.GetString(ea.Body.ToArray());

        try
        {
            await ConsumeMessageAsync(ea.RoutingKey, json);
            await _channel!.BasicAckAsync(ea.DeliveryTag, false);
        }
        catch
        {
            await _channel!.BasicNackAsync(ea.DeliveryTag, false, false);
        }
    }

    /// <summary>
    /// Deserializa y enruta el mensaje al consumidor correspondiente según la cola de origen.
    /// </summary>
    private async Task ConsumeMessageAsync(string routingKey, string json)
    {
        using var scope = _services.CreateScope();
        
        if (routingKey == _queueName)
        {
            var msg = JsonSerializer.Deserialize<SubscriptionCreatedMessage>(json);
            var handler = scope.ServiceProvider.GetRequiredService<SubscriptionConsumer>();
            await handler.Consume(msg!);
        }
        else
        {
            var msg = JsonSerializer.Deserialize<WebhookNotificationMessage>(json);
            var handler = scope.ServiceProvider.GetRequiredService<WebhookNotificationConsumer>();
            await handler.Consume(msg!);
        }
    }

    /// <summary>
    /// Cierra las conexiones de forma ordenada al detener el servicio.
    /// </summary>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel != null) await _channel.CloseAsync();
        if (_connection != null) await _connection.CloseAsync();
        await base.StopAsync(cancellationToken);
    }
}
