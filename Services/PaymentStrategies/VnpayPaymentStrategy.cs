using RentalVehicleService.Models;

namespace RentalVehicleService.Services.PaymentStrategies
{
    public class VnpayPaymentStrategy : IPaymentStrategy
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public VnpayPaymentStrategy(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public string Name => "VNPay";

        public Task<string> ProcessPaymentAsync(Rental rental, decimal amount)
        {
            var context = _httpContextAccessor.HttpContext;
            var request = context?.Request;
            var baseUrl = request != null ? $"{request.Scheme}://{request.Host}" : "";
            var returnUrl = $"{baseUrl}/Rental/PaymentCallback";

            var vnpay = new VnPayLibrary();

            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
            vnpay.AddRequestData("vnp_Amount", ((long)(amount * 100)).ToString()); // x100 theo format của VNPAY
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", VnPayLibrary.GetIpAddress(context));
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toan chuyen di {rental.RentalId}");
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
            vnpay.AddRequestData("vnp_TxnRef", $"{rental.RentalId}_{DateTime.Now.Ticks}"); // Tạo mã duy nhất tránh trùng lặp

            var paymentUrl = vnpay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);

            return Task.FromResult(paymentUrl);
        }
    }
}