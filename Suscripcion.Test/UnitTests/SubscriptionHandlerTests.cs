using FluentAssertions;
using Moq;
using Suscripcion.Application.DTOs;
using Suscripcion.Application.Interfaces;
using Suscripcion.Application.Suscripciones.Handlers;
using Suscripcion.Domain.Entities;
using Suscripcion.Domain.Interfaces;
using Xunit;

namespace Suscripcion.Test.UnitTests;

public class SubscriptionHandlerTests
{
    private readonly Mock<ISuscripcionRepository> _repositoryMock;
    private readonly Mock<IRabbitMqPublisher> _publisherMock;
    private readonly SubscriptionHandler _handler;

    public SubscriptionHandlerTests()
    {
        _repositoryMock = new Mock<ISuscripcionRepository>();
        _publisherMock = new Mock<IRabbitMqPublisher>();
        _handler = new SubscriptionHandler(_repositoryMock.Object, _publisherMock.Object);
    }

    [Fact]
    public async Task Subscribe_ValidRequest_ReturnsProcessing()
    {
        var userId = 1;
        var request = new SubscribeRequestDto(userId, "CreditCard");
        _repositoryMock.Setup(x => x.GetUserByIdAsync(userId)).ReturnsAsync(new User("John", "john@test.com"));
        _repositoryMock.Setup(x => x.GetSubscriptionWithUserAsync(userId)).ReturnsAsync((null, null));

        var result = await _handler.Subscribe(request);

        result.Status.Should().Be("Procesando");
        _publisherMock.Verify(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task Subscribe_UserNotFound_ReturnsUserNotFound()
    {
        var userId = 999;
        var request = new SubscribeRequestDto(userId, "CreditCard");
        _repositoryMock.Setup(x => x.GetUserByIdAsync(userId)).ReturnsAsync((User?)null);

        var result = await _handler.Subscribe(request);

        result.Status.Should().Be("Usuario no encontrado");
        _publisherMock.Verify(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task Subscribe_UserAlreadySubscribed_ReturnsAlreadyHasSubscription()
    {
        var userId = 1;
        var request = new SubscribeRequestDto(userId, "CreditCard");
        var activeSubscription = new Subscription(userId);
        activeSubscription.Activate(); // Marcar como activa
        
        _repositoryMock.Setup(x => x.GetUserByIdAsync(userId)).ReturnsAsync(new User("John", "john@test.com"));
        _repositoryMock.Setup(x => x.GetSubscriptionWithUserAsync(userId)).ReturnsAsync((activeSubscription, new User("John", "john@test.com")));

        var result = await _handler.Subscribe(request);

        result.Status.Should().Be("Ya tienes una suscripción activa");
        _publisherMock.Verify(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task Cancel_SubscriptionExists_ReturnsTrue()
    {
        var userId = 1;
        var subscription = new Subscription(userId);
        _repositoryMock.Setup(x => x.GetSubscriptionWithUserAsync(userId)).ReturnsAsync((subscription, new User("John", "john@test.com")));

        var result = await _handler.Cancel(userId);

        result.Should().BeTrue();
        _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Cancel_SubscriptionNotFound_ReturnsFalse()
    {
        _repositoryMock.Setup(x => x.GetSubscriptionWithUserAsync(999)).ReturnsAsync((null, null));

        var result = await _handler.Cancel(999);

        result.Should().BeFalse();
        _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Get_SubscriptionExists_ReturnsSubscriptionDto()
    {
        var subscription = new Subscription(1);
        subscription.Activate(); // Activar para el test
        var user = new User("John", "john@test.com");
        _repositoryMock.Setup(x => x.GetSubscriptionWithUserAsync(1)).ReturnsAsync((subscription, user));

        var result = await _handler.Get(1);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(1);
        result.UserEmail.Should().Be("john@test.com");
        result.Status.Should().Be("Active");
    }

    [Fact]
    public async Task Get_SubscriptionNotFound_ReturnsNull()
    {
        _repositoryMock.Setup(x => x.GetSubscriptionWithUserAsync(999)).ReturnsAsync((null, null));

        var result = await _handler.Get(999);

        result.Should().BeNull();
    }
}
