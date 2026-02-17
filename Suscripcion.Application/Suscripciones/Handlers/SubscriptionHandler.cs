using Suscripcion.Application.DTOs;
using Suscripcion.Application.Interfaces;
using Suscripcion.Application.Messages;
using Suscripcion.Domain.Interfaces;

namespace Suscripcion.Application.Suscripciones.Handlers;

/// <summary>
/// Manejador de operaciones de suscripciones: crear, consultar y cancelar.
/// </summary>
public class SubscriptionHandler
{
    private readonly ISuscripcionRepository _repository;
    private readonly IRabbitMqPublisher _publisher;

    /// <summary>
    /// Inicializa el manejador con el repositorio y el publicador de mensajes.
    /// </summary>
    public SubscriptionHandler(ISuscripcionRepository repository, IRabbitMqPublisher publisher)
    {
        _repository = repository;
        _publisher = publisher;
    }

    /// <summary>
    /// Crea una nueva suscripción de forma asíncrona mediante RabbitMQ.
    /// Valida que el usuario exista y no tenga una suscripción activa.
    /// </summary>
    /// <returns>Respuesta con estado "Processing" si se encola correctamente.</returns>
    public async Task<SubscribeResponseDto> Subscribe(SubscribeRequestDto request)
    {
        var user = await _repository.GetUserByIdAsync(request.UserId);
        if (user == null)
            return new SubscribeResponseDto("Usuario no encontrado");

        var (existingSubscription, _) = await _repository.GetSubscriptionWithUserAsync(request.UserId);
        if (existingSubscription != null && existingSubscription.Status == Domain.Enums.SuscripcionStatus.Active)
            return new SubscribeResponseDto("Ya tienes una suscripción activa");

        await _publisher.PublishAsync("subscription-queue", new SubscriptionCreatedMessage
        {
            UserId = request.UserId,
            PaymentMethod = request.PaymentMethod
        });

        return new SubscribeResponseDto("Procesando");
    }

    /// <summary>
    /// Obtiene la suscripción de un usuario con su información asociada.
    /// </summary>
    /// <returns>Datos de la suscripción o null si no existe.</returns>
    public async Task<GetSubscriptionResponseDto?> Get(int userId)
    {
        var (subscription, user) = await _repository.GetSubscriptionWithUserAsync(userId);

        if (subscription == null)
            return null;

        return new GetSubscriptionResponseDto(
            subscription.Id,
            subscription.UserId,
            user?.Email ?? "N/A",
            subscription.Status.ToString(),
            subscription.CreatedAt
        );
    }

    /// <summary>
    /// Cancela la suscripción de un usuario.
    /// </summary>
    /// <returns>True si se canceló exitosamente, false si no existe la suscripción.</returns>
    public async Task<bool> Cancel(int userId)
    {
        var (subscription, _) = await _repository.GetSubscriptionWithUserAsync(userId);

        if (subscription == null)
            return false;

        subscription.Cancel();
        await _repository.SaveChangesAsync();

        return true;
    }
}
