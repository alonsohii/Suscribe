using FluentAssertions;
using Moq;
using Suscripcion.Application.DTOs;
using Suscripcion.Application.Users;
using Suscripcion.Domain.Entities;
using Suscripcion.Domain.Interfaces;
using Xunit;

namespace Suscripcion.Test.UnitTests;

public class RegisterUserHandlerTests
{
    private readonly Mock<ISuscripcionRepository> _repositoryMock;
    private readonly RegisterUserHandler _handler;

    public RegisterUserHandlerTests()
    {
        _repositoryMock = new Mock<ISuscripcionRepository>();
        _handler = new RegisterUserHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsSuccessWithUserId()
    {
        // Arrange
        var request = new RegisterUserRequestDto("John Doe", "john@test.com");
        _repositoryMock.Setup(x => x.GetUserByEmailAsync(request.Email)).ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(request);

        // Assert
        result.Message.Should().Contain("exitosamente");
        _repositoryMock.Verify(x => x.AddUser(It.Is<User>(u => u.Name == "John Doe" && u.Email == "john@test.com")), Times.Once);
        _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_EmailAlreadyExists_ReturnsErrorMessage()
    {
        // Arrange
        var request = new RegisterUserRequestDto("John Doe", "existing@test.com");
        var existingUser = new User("Existing User", "existing@test.com");
        _repositoryMock.Setup(x => x.GetUserByEmailAsync(request.Email)).ReturnsAsync(existingUser);

        // Act
        var result = await _handler.Handle(request);

        // Assert
        result.UserId.Should().Be(0);
        result.Message.Should().Contain("ya registrado");
        _repositoryMock.Verify(x => x.AddUser(It.IsAny<User>()), Times.Never);
    }
}
