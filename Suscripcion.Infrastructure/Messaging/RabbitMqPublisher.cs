using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Suscripcion.Application.Interfaces;

namespace Suscripcion.Infrastructure.Messaging;

/// <summary>
/// Publicador de mensajes a RabbitMQ.
/// </summary>
public class RabbitMqPublisher : IRabbitMqPublisher
{
    private readonly IConnectionFactory _factory;

    /// <summary>
    /// Inicializa el publicador con la fábrica de conexiones de RabbitMQ.
    /// </summary>
    public RabbitMqPublisher(IConnectionFactory factory) => _factory = factory;

    /// <summary>
    /// Publica un mensaje genérico en la cola especificada.
    /// Serializa el mensaje a JSON y lo envía de forma asíncrona.
    /// </summary>
    public async Task PublishAsync<T>(string queue, T message)
    {
        using var connection = await _factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();
        
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);
        
        await channel.BasicPublishAsync("", queue, body);
    }
}
