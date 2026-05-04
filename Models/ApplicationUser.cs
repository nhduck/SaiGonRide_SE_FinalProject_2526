using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace RentalVehicleService.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string UserType { get; set; } = "Local"; // Local or Tourist

        // Email verification fields
        public string? EmailVerificationCode { get; set; }
        public DateTime? VerificationCodeExpires { get; set; }

        // Local User Specific
        [StringLength(20)]
        public string? CCCD { get; set; }

        // Tourist Specific
        [StringLength(50)]
        public string? PassportNumber { get; set; }

        [StringLength(100)]
        public string? Nationality { get; set; }
    }
}
