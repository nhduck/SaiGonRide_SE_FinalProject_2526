using RentalVehicleService.Models;

namespace RentalVehicleService.Services.PaymentStrategies
{
    public interface IPaymentStrategy
    {
        string Name { get; }
        Task<string> ProcessPaymentAsync(Rental rental, decimal amount);
    }
}
