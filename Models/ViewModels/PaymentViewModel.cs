namespace RentalVehicleService.Models
{
    public class PaymentViewModel
    {
        //thông tin khách
        public string? CustomerName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }

        //thông tin chuyến đi
        public int RentalId { get; set; }
        public string? EndStationName { get; set; }
        public string? EndStationAddress { get; set; }
        public string? VehicleModel { get; set; }
        public int? VehicleBattery { get; set; }

        //Thờ gian, chi phí á
        public DateTime StartTime { get; set; }
        public int TotalMinutes { get; set; }
        public decimal? FinalFare { get; set; }
        public decimal? DiscountAmount { get; set; }
    }
}