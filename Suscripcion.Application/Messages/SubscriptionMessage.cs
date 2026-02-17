namespace Suscripcion.Application.Messages;

public record SubscriptionCreatedMessage
{
    public int UserId { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;
}
