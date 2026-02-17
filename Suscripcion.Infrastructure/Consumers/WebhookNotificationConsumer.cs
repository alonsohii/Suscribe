using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Suscripcion.Application.Interfaces;
using Suscripcion.Application.Messages;

namespace Suscripcion.Infrastructure.Consumers;

/// <summary>
/// Consumidor de notificaciones webhook con política de reintentos automáticos.
/// </summary>
public class WebhookNotificationConsumer
{
    private readonly IWebhookNotifier _webhookNotifier;
    private readonly ILogger<WebhookNotificationConsumer> _logger;

    /// <summary>
    /// Inicializa el consumidor con el notificador de webhooks y el logger.
    /// </summary>
    public WebhookNotificationConsumer(
        IWebhookNotifier webhookNotifier,
        ILogger<WebhookNotificationConsumer> logger)
    {
        _webhookNotifier = webhookNotifier;
        _logger = logger;
    }

    /// <summary>
    /// Consume un mensaje de notificación webhook con reintentos automáticos.
    /// Política: 3 intentos con 5 segundos de espera entre cada uno.
    /// </summary>
    public async Task Consume(WebhookNotificationMessage message)
    {
        var retryPipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(5)
            })
            .Build();

        await retryPipeline.ExecuteAsync(async ct =>
        {
            _logger.LogInformation("Enviando webhook para UserId: {UserId}", message.UserId);
            await _webhookNotifier.NotifyAsync(message.Message, message.IdempotencyKey);
            _logger.LogInformation("Webhook enviado exitosamente para UserId: {UserId}", message.UserId);
        });
    }
}
