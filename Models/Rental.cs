using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentalVehicleService.Models
{
    public enum RentalStatus
    {
        Active,
        PendingPayment,
        Completed,
        Cancelled
    }

    public class Rental
    {
        [Key]
        public int RentalId { get; set; }

        [Required]
        public string UserId { get; set; } = "";

        [Required]
        public int VehicleId { get; set; }

        [Required]
        public int StartStationId { get; set; }

        public int? EndStationId { get; set; }

        public DateTime StartTime { get; set; } = DateTime.Now;

        public DateTime? EndTime { get; set; }

        public RentalStatus Status { get; set; } = RentalStatus.Active;

        [Column(TypeName = "decimal(18,2)")]
        public decimal FinalFare { get; set; } = 0;

        public VehicleType VehicleType { get; set; }

        // Navigation properties
        [ForeignKey("VehicleId")]
        public Vehicle? Vehicle { get; set; }

        [ForeignKey("StartStationId")]
        public Station? StartStation { get; set; }

        [ForeignKey("EndStationId")]
        public Station? EndStation { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiscountAmount { get; set; }

        // Payment tracking
        public string? PaymentMethod { get; set; } // VNPay, PayPal, Card, etc.
        public string? PaymentTransactionId { get; set; } // Store PayPal Order ID, VNPay transaction ID, etc.
        public DateTime? PaymentCompletedTime { get; set; }
    }
}
