using Microsoft.EntityFrameworkCore;
using Suscripcion.Application.Interfaces;
using Suscripcion.Application.Messages;
using Suscripcion.Domain.Entities;
using Suscripcion.Domain.Enums;
using Suscripcion.Domain.Interfaces;
using Suscripcion.Infrastructure.Persistence;

namespace Suscripcion.Infrastructure.Consumers;

public class SubscriptionConsumer
{
    private readonly ISuscripcionRepository _repository;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IRabbitMqPublisher _publisher;
    private readonly AppDbContext _context;

    public SubscriptionConsumer(
        ISuscripcionRepository repository, 
        IPaymentGateway paymentGateway,
        IRabbitMqPublisher publisher,
        AppDbContext context)
    {
        _repository = repository;
        _paymentGateway = paymentGateway;
        _publisher = publisher;
        _context = context;
    }

    /// <summary>
    /// Procesa un mensaje de creación de suscripción desde RabbitMQ.
    /// Valida el usuario, verifica duplicados, procesa el pago y publica notificación webhook.
    /// </summary>
    /// <param name="message">Mensaje con userId y método de pago.</param>
    public async Task Consume(SubscriptionCreatedMessage message)
    {
        User? user = await _repository.GetUserByIdAsync(message.UserId);
        if (user == null) return;

        // Validar si ya existe (idempotencia)
        var existing = await _repository.GetByUserIdAsync(message.UserId);
        if (existing != null) return; // Ya procesado, evita duplicados

        // PRODUCCIÓN: Validar también en Redis para evitar duplicados si BD está caída
        // var redisKey = $"payment:{message.UserId}";
        // if (await _redis.ExistsAsync(redisKey)) return; // Ya procesado en Redis

        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // 1. Guardar suscripción con estado Pending
            var subscription = new Subscription(message.UserId, SuscripcionStatus.Pending);
            _repository.Add(subscription);
            await _repository.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        // 2. Procesar pago FUERA de la transacción
        bool paymentSuccess = await _paymentGateway.PayAsync(message.PaymentMethod);

        // PRODUCCIÓN: Guardar en Redis si BD falla (expira en 24 horas)
        // await _redis.SetAsync($"payment:{message.UserId}", paymentSuccess);

        // 3. Actualizar estado según resultado
        using var updateTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var subscription = await _repository.GetByUserIdAsync(message.UserId);
            if (subscription == null) return;

            if (paymentSuccess)
            {
                subscription.Activate();
                await _repository.SaveChangesAsync();
                await updateTransaction.CommitAsync();

                // 4. Publicar webhook solo si pago exitoso
                await _publisher.PublishAsync("webhook-notification-queue", new WebhookNotificationMessage
                {
                    UserId = message.UserId,
                    Message = $"Payment successful for UserId: {message.UserId}"
                });
            }
            else
            {
                subscription.MarkAsFailed();
                await _repository.SaveChangesAsync();
                await updateTransaction.CommitAsync();
            }
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
