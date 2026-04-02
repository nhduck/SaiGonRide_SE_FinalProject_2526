using System.ComponentModel.DataAnnotations;

namespace RentalVehicleService.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email cannot be empty")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Password cannot be empty")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = "";

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
}
