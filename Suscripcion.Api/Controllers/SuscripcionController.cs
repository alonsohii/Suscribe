using Microsoft.AspNetCore.Mvc;
using Suscripcion.Application.DTOs;
using Suscripcion.Application.Suscripciones.Handlers;
using Suscripcion.Domain.Interfaces;

namespace Suscripcion.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SuscripcionController : ControllerBase
{
    private readonly SubscriptionHandler _subscriptionHandler;

    public SuscripcionController(SubscriptionHandler subscriptionHandler)
    {
        _subscriptionHandler = subscriptionHandler;
    }

    /// <summary>
    /// Crea una nueva suscripción para un usuario y procesa el pago de forma asíncrona vía RabbitMQ.
    /// </summary>
    /// <param name="request">Solicitud de suscripción con userId y método de pago.</param>
    /// <returns>Respuesta aceptada con detalles de la suscripción.</returns>
    [HttpPost("subscribe")]
    public async Task<ActionResult<SubscribeResponseDto>> Subscribe(SubscribeRequestDto request)
    {
        var result = await _subscriptionHandler.Subscribe(request);
        return Accepted(result);
    }

    /// <summary>
    /// Obtiene los detalles de la suscripción de un usuario específico.
    /// </summary>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <returns>Detalles de la suscripción o 404 si no existe.</returns>
    [HttpGet("{userId}")]
    public async Task<ActionResult<GetSubscriptionResponseDto>> GetSubscription(int userId)
    {
        var subscription = await _subscriptionHandler.Get(userId);

        if (subscription == null)
            return NotFound(new { message = "Subscription not found" });

        return Ok(subscription);
    }

    /// <summary>
    /// Obtiene todas las suscripciones con información de usuario asociada.
    /// </summary>
    /// <returns>Lista de todas las suscripciones incluyendo email y estado.</returns>
    [HttpGet]
    public async Task<ActionResult> GetAll([FromServices] ISuscripcionRepository repo)
    {
        var data = await repo.GetAllWithUsersAsync();
        var result = data.Select(x => new
        {
            x.subscription.Id,
            x.subscription.UserId,
            UserEmail = x.user?.Email,
            Status = x.subscription.Status.ToString(),
            x.subscription.CreatedAt
        });
        
        return Ok(result);
    }

    /// <summary>
    /// Cancela la suscripción de un usuario.
    /// </summary>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <returns>200 OK si se canceló exitosamente, 404 si no existe.</returns>
    [HttpPost("{userId}/cancel")]
    public async Task<ActionResult> CancelSubscription(int userId)
    {
        var success = await _subscriptionHandler.Cancel(userId);

        if (!success)
            return NotFound(new { message = "Subscription not found" });

        return Ok(new { message = "Subscription cancelled successfully" });
    }
}
