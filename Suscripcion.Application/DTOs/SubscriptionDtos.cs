namespace Suscripcion.Application.DTOs;

public record SubscribeRequestDto(int UserId, string PaymentMethod);

public record SubscribeResponseDto(string Status);

public record GetSubscriptionResponseDto(
    int SubscriptionId,
    int UserId,
    string UserEmail,
    string Status,
    DateTime CreatedAt
);
