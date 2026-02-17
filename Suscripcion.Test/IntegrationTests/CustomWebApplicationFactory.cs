using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Suscripcion.Application.Interfaces;

namespace Suscripcion.Test.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remover el RabbitMqPublisher real
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IRabbitMqPublisher));
            
            if (descriptor != null)
                services.Remove(descriptor);

            // Agregar un mock que no falla
            var mockPublisher = new Mock<IRabbitMqPublisher>();
            mockPublisher.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>()))
                .Returns(Task.CompletedTask);
            
            services.AddSingleton(mockPublisher.Object);
        });
    }
}
