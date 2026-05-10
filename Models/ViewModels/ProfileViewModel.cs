using System.ComponentModel.DataAnnotations;

namespace RentalVehicleService.Models.ViewModels
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Full Name cannot be empty")]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        public string UserType { get; set; } = "Local";

        [Display(Name = "Phone Number")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "National ID (CCCD)")]
        [RegularExpression(@"^\d{9}$|^\d{12}$", ErrorMessage = "National ID must be 9 or 12 digits")]
        public string? CCCD { get; set; }

        [Display(Name = "Passport Number")]
        public string? PassportNumber { get; set; }

        [Display(Name = "Nationality")]
        public string? Nationality { get; set; }

        // Activity Stats
        public int TotalRides { get; set; } = 0;
        public double TotalDistance { get; set; } = 0;
        public double TotalCO2Saved { get; set; } = 0;
    }
}
