using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Suscripcion.Application.Interfaces;
using Suscripcion.Application.Messages;
using Suscripcion.Infrastructure.Consumers;
using Xunit;

namespace Suscripcion.Test.UnitTests;

public class WebhookNotificationConsumerTests
{
    private readonly Mock<IWebhookNotifier> _webhookMock;
    private readonly Mock<ILogger<WebhookNotificationConsumer>> _loggerMock;
    private readonly WebhookNotificationConsumer _consumer;

    public WebhookNotificationConsumerTests()
    {
        _webhookMock = new Mock<IWebhookNotifier>();
        _loggerMock = new Mock<ILogger<WebhookNotificationConsumer>>();
        _consumer = new WebhookNotificationConsumer(_webhookMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Consume_ValidMessage_CallsWebhookNotifier()
    {
        var message = new WebhookNotificationMessage
        {
            UserId = 1,
            Message = "Payment successful for UserId: 1"
        };

        _webhookMock.Setup(x => x.NotifyAsync(message.Message, It.IsAny<Guid?>())).Returns(Task.CompletedTask);

        await _consumer.Consume(message);

        _webhookMock.Verify(x => x.NotifyAsync(message.Message, It.IsAny<Guid?>()), Times.Once);
    }

    [Fact]
    public async Task Consume_WebhookFails_ThrowsException()
    {
        var message = new WebhookNotificationMessage
        {
            UserId = 1,
            Message = "Test message"
        };

        _webhookMock.Setup(x => x.NotifyAsync(It.IsAny<string>(), It.IsAny<Guid?>()))
            .ThrowsAsync(new HttpRequestException("Webhook service unavailable"));

        var act = async () => await _consumer.Consume(message);

        await act.Should().ThrowAsync<HttpRequestException>();
    }
}
