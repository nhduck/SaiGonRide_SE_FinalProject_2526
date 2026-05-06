using RentalVehicleService.Models;

namespace RentalVehicleService.Services.PaymentStrategies
{
    public class PaypalPaymentStrategy : IPaymentStrategy
    {
        public string Name => "PayPal";

        public Task<string> ProcessPaymentAsync(Rental rental, decimal amount)
        {
            // For the purpose of this project, we simulate the PayPal flow.
            // In a real scenario, this would redirect to PayPal's sandbox/live URL.
            // Here we redirect back to a simplified callback.
            return Task.FromResult($"/Rental/PaypalCallback?rentalId={rental.RentalId}");
        }
    }
}
