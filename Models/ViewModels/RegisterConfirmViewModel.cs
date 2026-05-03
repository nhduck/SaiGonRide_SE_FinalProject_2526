using System.ComponentModel.DataAnnotations;

namespace RentalVehicleService.Models.ViewModels
{
    public class RegisterConfirmViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mã xác nhận.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã xác nhận phải có 6 chữ số.")]
        public string Code { get; set; } = string.Empty;
    }
}
