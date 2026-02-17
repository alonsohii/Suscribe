namespace Suscripcion.Application.Messages;

public record WebhookNotificationMessage
{
    public Guid IdempotencyKey { get; init; } = Guid.NewGuid();
    public int UserId { get; init; }
    public string Message { get; init; } = string.Empty;
}
