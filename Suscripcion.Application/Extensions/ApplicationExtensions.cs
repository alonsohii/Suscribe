using Microsoft.Extensions.DependencyInjection;
using Suscripcion.Application.Suscripciones.Handlers;
using Suscripcion.Application.Users;

namespace Suscripcion.Application.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<SubscriptionHandler>();
        services.AddScoped<RegisterUserHandler>();

        return services;
    }
}
