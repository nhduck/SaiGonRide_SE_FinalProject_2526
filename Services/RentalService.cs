using RentalVehicleService.Data;
using RentalVehicleService.Models;

namespace RentalVehicleService.Services
{
    public class RentalService
    {
        private readonly ApplicationDbContext _context;

        public RentalService(ApplicationDbContext context)
        {
            _context = context;
        }

        public decimal CalculateFare(DateTime startTime, DateTime endTime, decimal pricePerMin)
        {
            // Tính toán khoảng thời gian thuê
            TimeSpan duration = endTime - startTime;
            double totalMinutes = Math.Ceiling(duration.TotalMinutes);

            // Đảm bảo không bị âm
            if (totalMinutes < 0) totalMinutes = 0;

            // Nhân tổng số phút với giá mỗi phút đã nhận từ tham số
            return (decimal)totalMinutes * pricePerMin;
        }

        public decimal CheckDiscount(int endStationId)
        {
            var station = _context.Stations.Find(endStationId);
            if (station == null || !station.IsActive)
            {
                return 0m;
            }
            if (station.IsLowInventory)
            {
                return 0.15m;
            }
            return 0m;
        }

        public decimal ProcessFinalBill(Rental rental, int endStationId, DateTime? endTime = null)
        {
            var vehicle = _context.Vehicles.Find(rental.VehicleId);
            if (vehicle == null) return 0m;

            DateTime end = endTime ?? DateTime.Now;
            decimal baseFare = CalculateFare(rental.StartTime, end, (decimal)vehicle.Price);
            decimal discountRate = CheckDiscount(endStationId);

            decimal discountAmount = baseFare * discountRate;
            decimal finalFare = baseFare - discountAmount;

            rental.DiscountAmount = discountAmount;

            return finalFare;
        }

        public decimal ApplyCoupon(decimal currentFare, string couponCode)
        {
            if (string.IsNullOrEmpty(couponCode)) return currentFare;

            if (couponCode.ToUpper() == "SAIGONGREEN20")
            {
                decimal newFare = currentFare - 10000;
                return newFare < 0 ? 0 : newFare;
            }

            return currentFare;
        }
    }
}
