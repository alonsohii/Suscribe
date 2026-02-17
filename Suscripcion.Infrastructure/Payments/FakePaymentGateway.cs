using Suscripcion.Application.Interfaces;

namespace Suscripcion.Infrastructure.Payments
{
    public class FakePaymentGateway : IPaymentGateway
    {
        private readonly string[] _validMethods = { "credit_card", "creditcard", "paypal" };

        public Task<bool> PayAsync(string method)
        {
            if (string.IsNullOrWhiteSpace(method))
                return Task.FromResult(false);

            return Task.FromResult(_validMethods.Contains(method.ToLower()));
        }
    }
}
