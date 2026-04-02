using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentalVehicleService.Models
{
    public enum VehicleState
    {
        Available,    // Sẵn sàng
        Rented,       // Đang thuê
        Charging,     // Đang sạc
        Maintenance   // Bảo trì
    }

    public enum VehicleType
    {
        Electric,
        Standard
    }

    public class Vehicle
    {
        [Key]
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "Vehicle model cannot be empty")]
        [Display(Name = "Vehicle Model")]
        public string? VehicleModel { get; set; }

        [Required]
        [Display(Name = "Rental Price (VND/min)")]
        public double Price { get; set; }

        [Range(-1, 100, ErrorMessage = "Battery must be between 0% and 100%")]
        [Display(Name = "Battery (%)")]
        public int BatteryPercentage { get; set; }

        [Display(Name = "State")]
        public VehicleState State { get; set; }

        [Display(Name = "Type")]
        public VehicleType Type { get; set; }

        [Display(Name = "Last Maintenance Date")]
        public DateTime LastMaintenance { get; set; }

        [Display(Name = "Current Station")]
        public int? CurrentStationId { get; set; }

        // Navigation property
        [ForeignKey("CurrentStationId")]
        public Station? CurrentStation { get; set; }

        // Chỉ dùng để kiểm tra, không lưu Database
        [NotMapped]
        public bool IsReadyForRent => BatteryPercentage > 20;
    }
}
