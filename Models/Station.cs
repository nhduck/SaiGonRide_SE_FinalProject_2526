using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentalVehicleService.Models
{
    public class Station
    {
        [Key]
        public int StationId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = "";

        [Required]
        [StringLength(255)]
        public string Address { get; set; } = "";

        [Range(1, 1000)]
        public int TotalCapacity { get; set; }

        public int CurrentCount { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        // Logic tính toán: Tỷ lệ lấp đầy (không lưu vào DB)
        [NotMapped]
        public double FillRate => TotalCapacity > 0 ? (double)CurrentCount / TotalCapacity : 0;

        // Logic nghiệp vụ: Cảnh báo bãi rỗng (< 20% sẽ được giảm giá 15%)
        [NotMapped]
        public bool IsLowInventory => FillRate < 0.20;
    }
}