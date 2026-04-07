using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentalVehicleService.Models
{
    public class Station
    {
        [Key]
        public int StationId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public virtual ICollection<RentalVehicleService.Models.Vehicle> Vehicles { get; set; } = new List<RentalVehicleService.Models.Vehicle>();

        [Required(ErrorMessage = "Station name cannot be empty")]
        [StringLength(100)]
        [Display(Name = "Station Name")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Address cannot be empty")]
        [StringLength(255)]
        [Display(Name = "Address")]
        public string Address { get; set; } = "";

        [Range(1, 1000, ErrorMessage = "Capacity must be between 1 and 1000")]
        [Display(Name = "Total Capacity")]
        public int TotalCapacity { get; set; }

        [Display(Name = "Current Vehicles Count")]
        public int CurrentCount { get; set; } = 0;

        [Display(Name = "Active Status")]
        public bool IsActive { get; set; } = true;

        // Logic tính toán: Tỷ lệ lấp đầy
        [NotMapped]
        public double FillRate => TotalCapacity > 0 ? (double)CurrentCount / TotalCapacity : 0;

        // Logic nghiệp vụ: Cảnh báo bãi rỗng
        [NotMapped]
        public bool IsLowInventory => FillRate < 0.20;
    }
}