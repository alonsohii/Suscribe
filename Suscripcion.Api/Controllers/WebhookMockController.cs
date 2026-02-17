using Microsoft.AspNetCore.Mvc;

namespace Suscripcion.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhookMockController : ControllerBase
{
    private static readonly List<object> _receivedWebhooks = new();
    private readonly IConfiguration _configuration;

    public WebhookMockController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Endpoint mock para recibir webhooks (solo para desarrollo).
    /// </summary>
    [HttpPost("receive")]
    public ActionResult ReceiveWebhook([FromBody] object payload)
    {
        var simulateError = _configuration.GetValue<bool>("Webhook:SimulateError");
        
        if (simulateError)
        {
            Console.WriteLine($"[WEBHOOK ERROR SIMULADO] {DateTime.UtcNow}: Retornando 500");
            return StatusCode(500, new { error = "Simulated webhook failure" });
        }

        _receivedWebhooks.Add(new 
        { 
            timestamp = DateTime.UtcNow,
            payload 
        });

        Console.WriteLine($"[WEBHOOK RECIBIDO] {DateTime.UtcNow}: {System.Text.Json.JsonSerializer.Serialize(payload)}");
        
        return Ok(new { message = "Webhook received successfully" });
    }

    /// <summary>
    /// Ver estado actual de la simulación de errores.
    /// </summary>
    [HttpGet("error-status")]
    public ActionResult GetErrorStatus()
    {
        var simulateError = _configuration.GetValue<bool>("Webhook:SimulateError");
        return Ok(new 
        { 
            simulateError,
            message = simulateError ? "Errores activados - Cambia 'Webhook:SimulateError' a false en appsettings.json" : "Errores desactivados"
        });
    }

    /// <summary>
    /// Ver todos los webhooks recibidos.
    /// </summary>
    [HttpGet("received")]
    public ActionResult GetReceivedWebhooks()
    {
        return Ok(_receivedWebhooks);
    }

    /// <summary>
    /// Limpiar webhooks recibidos.
    /// </summary>
    [HttpDelete("clear")]
    public ActionResult ClearWebhooks()
    {
        _receivedWebhooks.Clear();
        return Ok(new { message = "Webhooks cleared" });
    }
}
