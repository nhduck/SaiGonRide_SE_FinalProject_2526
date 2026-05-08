using System.ComponentModel.DataAnnotations;

namespace RentalVehicleService.Models
{
    public class BugReport
    {
        public enum BugStatus
        {
            New,
            InProgress,
            Resolved,
            Closed
        }

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề lỗi")]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email của bạn")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(255)]
        public string ReporterEmail { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public BugStatus Status { get; set; } = BugStatus.New;
    }
}
