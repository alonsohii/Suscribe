using FluentAssertions;
using Suscripcion.Domain.Entities;
using Suscripcion.Domain.Enums;
using Xunit;

namespace Suscripcion.Test.UnitTests;

public class SubscriptionTests
{
    [Fact]
    public void Constructor_CreatesSubscriptionWithPendingStatus()
    {
        // Arrange
        var userId = 1;

        // Act
        var subscription = new Subscription(userId);

        // Assert
        subscription.Status.Should().Be(SuscripcionStatus.Pending);
        subscription.UserId.Should().Be(userId);
        subscription.Id.Should().Be(0);
    }

    [Fact]
    public void Cancel_ChangesStatusToCancelled()
    {
        // Arrange
        var subscription = new Subscription(1);

        // Act
        subscription.Cancel();

        // Assert
        subscription.Status.Should().Be(SuscripcionStatus.Cancelled);
    }

    [Fact]
    public void CreatedAt_IsSetToUtcNow()
    {
        // Arrange & Act
        var before = DateTime.UtcNow;
        var subscription = new Subscription(1);
        var after = DateTime.UtcNow;

        // Assert
        subscription.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }
}

public class UserTests
{
    [Fact]
    public void Constructor_CreatesUserWithNameAndEmail()
    {
        // Arrange & Act
        var user = new User("John Doe", "john@test.com");

        // Assert
        user.Name.Should().Be("John Doe");
        user.Email.Should().Be("john@test.com");
        user.Id.Should().Be(0);
    }
}
