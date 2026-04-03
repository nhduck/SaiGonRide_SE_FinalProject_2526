using RentalVehicleService.Data;

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
            var vehicle = _context.Vehicles.Find(pricePerMin);
            if (vehicle == null) return 0;

            TimeSpan duration = endTime - startTime;
            double totalMinutes = Math.Ceiling(duration.TotalMinutes);
            if (totalMinutes < 0) totalMinutes = 0;

            decimal currentRate = (decimal)vehicle.Price;

            return (decimal)totalMinutes * currentRate;

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

        public decimal ProcessFinalBill(int rentalId, int endStationId)
        {
            var rental = _context.Rentals.Find(rentalId);
            var vehicle = _context.Vehicles.Find(rental.VehicleId);

            decimal baseFare = CalculateFare(rental.StartTime, DateTime.Now, (decimal)vehicle.Price);

            decimal discountRate = CheckDiscount(endStationId);

            decimal discountAmount = baseFare * discountRate;
            decimal finalFare = baseFare - discountAmount;

            return finalFare;
        }

    }
}
