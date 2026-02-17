using FluentAssertions;
using Suscripcion.Infrastructure.Payments;
using Xunit;

namespace Suscripcion.Test.UnitTests;

public class FakePaymentGatewayTests
{
    private readonly FakePaymentGateway _gateway;

    public FakePaymentGatewayTests()
    {
        _gateway = new FakePaymentGateway();
    }

    [Theory]
    [InlineData("credit_card")]
    [InlineData("CreditCard")]
    [InlineData("CREDIT_CARD")]
    [InlineData("paypal")]
    [InlineData("PayPal")]
    public async Task PayAsync_ValidPaymentMethod_ReturnsTrue(string paymentMethod)
    {
        var result = await _gateway.PayAsync(paymentMethod);
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid")]
    public async Task PayAsync_InvalidPaymentMethod_ReturnsFalse(string paymentMethod)
    {
        var result = await _gateway.PayAsync(paymentMethod);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task PayAsync_NullPaymentMethod_ReturnsFalse()
    {
        var result = await _gateway.PayAsync(null!);
        result.Should().BeFalse();
    }
}
