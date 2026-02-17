using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Suscripcion.Application.Interfaces;
using Suscripcion.Domain.Interfaces;
using Suscripcion.Infrastructure.Consumers;
using Suscripcion.Infrastructure.Messaging;
using Suscripcion.Infrastructure.Payments;
using Suscripcion.Infrastructure.Persistence;
using Suscripcion.Infrastructure.Webhooks;

namespace Suscripcion.Infrastructure.Extensions;

/// <summary>
/// Extensiones para configurar servicios de infraestructura y mensajería.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registra servicios de infraestructura: base de datos, repositorios, pagos y webhooks.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        String connString = config.GetConnectionString("DefaultConnection")!;
        
        if (string.IsNullOrEmpty(connString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' is null or empty.");
        }
        
        services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(connString, ServerVersion.AutoDetect(connString)));

        services.AddScoped<ISuscripcionRepository, SubscriptionRepository>();
        services.AddScoped<IPaymentGateway, FakePaymentGateway>();
        services.AddHttpClient<IWebhookNotifier, WebhookNotifier>();
        services.AddScoped<SubscriptionConsumer>();
        services.AddScoped<WebhookNotificationConsumer>();

        return services;
    }

    /// <summary>
    /// Registra servicios de mensajería: RabbitMQ publisher, consumer y configuración de conexión.
    /// </summary>
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration config)
    {
        var rabbitUri = Environment.GetEnvironmentVariable("RABBITMQ_PRIVATE_URL");
        var rabbit = config.GetSection("RabbitMq");
        
        if (!string.IsNullOrEmpty(rabbitUri))
        {
            services.AddSingleton<IConnectionFactory>(sp => new ConnectionFactory { Uri = new Uri(rabbitUri) });
        }
        else if (!string.IsNullOrEmpty(rabbit["Host"]))
        {
            services.AddSingleton<IConnectionFactory>(sp => new ConnectionFactory
            {
                HostName = rabbit["Host"],
                UserName = rabbit["Username"] ?? "guest",
                Password = rabbit["Password"] ?? "guest"
            });
        }
        else
        {
            return services;
        }
        
        services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
        services.AddHostedService<RabbitMqConsumerService>();

        return services;
    }
}
