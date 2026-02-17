using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Suscripcion.Application.Interfaces;
using Suscripcion.Application.Messages;
using Suscripcion.Domain.Entities;
using Suscripcion.Domain.Interfaces;
using Suscripcion.Infrastructure.Consumers;
using Suscripcion.Infrastructure.Persistence;
using Xunit;

namespace Suscripcion.Test.UnitTests;

public class SubscriptionConsumerTests
{
    private readonly Mock<ISuscripcionRepository> _repositoryMock;
    private readonly Mock<IPaymentGateway> _paymentMock;
    private readonly Mock<IRabbitMqPublisher> _publishMock;
    private readonly AppDbContext _context;
    private readonly SubscriptionConsumer _consumer;

    public SubscriptionConsumerTests()
    {
        _repositoryMock = new Mock<ISuscripcionRepository>();
        _paymentMock = new Mock<IPaymentGateway>();
        _publishMock = new Mock<IRabbitMqPublisher>();
        
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _context = new AppDbContext(options);
        
        _consumer = new SubscriptionConsumer(
            _repositoryMock.Object,
            _paymentMock.Object,
            _publishMock.Object,
            _context);
    }

    [Fact]
    public async Task Consume_SuccessfulPayment_CreatesSubscriptionAndPublishesWebhook()
    {
        var message = new SubscriptionCreatedMessage { UserId = 1, PaymentMethod = "CreditCard" };

        _repositoryMock.Setup(x => x.GetUserByIdAsync(1)).ReturnsAsync(new User("John", "john@test.com"));
        _repositoryMock.Setup(x => x.GetByUserIdAsync(1)).ReturnsAsync((Subscription?)null).Callback(() =>
        {
            // Simular que después del primer save, la suscripción existe
            _repositoryMock.Setup(x => x.GetByUserIdAsync(1)).ReturnsAsync(new Subscription(1));
        });
        _paymentMock.Setup(x => x.PayAsync("CreditCard")).ReturnsAsync(true);

        await _consumer.Consume(message);

        _repositoryMock.Verify(x => x.Add(It.IsAny<Subscription>()), Times.Once);
        _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.AtLeastOnce);
        _publishMock.Verify(x => x.PublishAsync<WebhookNotificationMessage>(It.IsAny<string>(), It.IsAny<WebhookNotificationMessage>()), Times.Once);
    }

    [Fact]
    public async Task Consume_UserNotFound_DoesNotCreateSubscription()
    {
        var message = new SubscriptionCreatedMessage { UserId = 999, PaymentMethod = "CreditCard" };
        _repositoryMock.Setup(x => x.GetUserByIdAsync(999)).ReturnsAsync((User?)null);

        await _consumer.Consume(message);

        _repositoryMock.Verify(x => x.Add(It.IsAny<Subscription>()), Times.Never);
        _paymentMock.Verify(x => x.PayAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Consume_PaymentFails_DoesNotCreateSubscription()
    {
        var message = new SubscriptionCreatedMessage { UserId = 1, PaymentMethod = "CreditCard" };
        _repositoryMock.Setup(x => x.GetUserByIdAsync(1)).ReturnsAsync(new User("John", "john@test.com"));
        _repositoryMock.Setup(x => x.GetByUserIdAsync(1)).ReturnsAsync((Subscription?)null).Callback(() =>
        {
            _repositoryMock.Setup(x => x.GetByUserIdAsync(1)).ReturnsAsync(new Subscription(1));
        });
        _paymentMock.Setup(x => x.PayAsync("CreditCard")).ReturnsAsync(false);

        await _consumer.Consume(message);

        _repositoryMock.Verify(x => x.Add(It.IsAny<Subscription>()), Times.Once);
        _publishMock.Verify(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<WebhookNotificationMessage>()), Times.Never);
    }

    [Fact]
    public async Task Consume_UserAlreadyHasSubscription_DoesNotCreateAnother()
    {
        var message = new SubscriptionCreatedMessage { UserId = 1, PaymentMethod = "CreditCard" };
        _repositoryMock.Setup(x => x.GetUserByIdAsync(1)).ReturnsAsync(new User("John", "john@test.com"));
        _paymentMock.Setup(x => x.PayAsync("CreditCard")).ReturnsAsync(true);

        await _consumer.Consume(message);

        _repositoryMock.Verify(x => x.Add(It.IsAny<Subscription>()), Times.Once);
        _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}
