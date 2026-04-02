using System.ComponentModel.DataAnnotations;

namespace RentalVehicleService.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Full Name cannot be empty")]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "Email cannot be empty")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Password cannot be empty")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Confirm Password cannot be empty")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = "";

        // User type: "Local" or "Tourist"
        [Required]
        public string UserType { get; set; } = "Local";

        // Local fields
        [Display(Name = "National ID (CCCD)")]
        [RegularExpression(@"^\d{9}$|^\d{12}$", ErrorMessage = "National ID must be 9 or 12 digits")]
        public string? CCCD { get; set; }

        [Display(Name = "Phone Number")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string? PhoneNumber { get; set; }

        // Tourist fields
        [Display(Name = "Passport Number")]
        public string? PassportNumber { get; set; }

        [Display(Name = "Nationality")]
        public string? Nationality { get; set; }
    }
}
