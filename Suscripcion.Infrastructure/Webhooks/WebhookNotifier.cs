using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Suscripcion.Application.Interfaces;

namespace Suscripcion.Infrastructure.Webhooks
{
    /// <summary>
    /// Notificador de webhooks mediante HTTP POST.
    /// </summary>
    public class WebhookNotifier : IWebhookNotifier
    {
        private readonly HttpClient _http;
        private readonly ILogger<WebhookNotifier> _logger;
        private readonly string _webhookUrl;

        /// <summary>
        /// Inicializa el notificador con el cliente HTTP, configuración y logger.
        /// </summary>
        public WebhookNotifier(HttpClient http, IConfiguration config, ILogger<WebhookNotifier> logger)
        {
            _http = http;
            _logger = logger;
            _webhookUrl = config["Webhook:Url"] ?? "https://webhook.site/test";
        }

        /// <summary>
        /// Envía una notificación webhook
        /// </summary>
        public async Task NotifyAsync(string msg, Guid? idempotencyKey = null)
        {
            try
            {
                var payload = new 
                { 
                    message = msg, 
                    timestamp = DateTime.UtcNow,
                    idempotencyKey = idempotencyKey?.ToString() ?? Guid.NewGuid().ToString()
                };
                var content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json");

                _logger.LogInformation("Sending webhook to {Url} with IdempotencyKey: {Key}", _webhookUrl, payload.idempotencyKey);
                var response = await _http.PostAsync(_webhookUrl, content);
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Webhook sent successfully");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Webhook failed: {Message}", ex.Message);
                throw; 
            }
        }
    }
}
