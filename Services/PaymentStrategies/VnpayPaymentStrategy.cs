using RentalVehicleService.Models;
using VNPAY;
using VNPAY.Models;
using VNPAY.Models.Enums;

namespace RentalVehicleService.Services.PaymentStrategies
{
    public class VnpayPaymentStrategy : IPaymentStrategy
    {
        private readonly IVnpayClient _vnpayClient;

        public VnpayPaymentStrategy(IVnpayClient vnpayClient)
        {
            _vnpayClient = vnpayClient;
        }

        public string Name => "VNPay";

        public Task<string> ProcessPaymentAsync(Rental rental, decimal amount)
        {
            var request = new VnpayPaymentRequest
            {
                Money = (double)amount,
                Description = $"Thanh toan chuyen di {rental.RentalId} tai SaigonRide",
                BankCode = BankCode.ANY
            };

            var paymentUrlInfo = _vnpayClient.CreatePaymentUrl(request);
            return Task.FromResult(paymentUrlInfo.Url);
        }
    }
}
