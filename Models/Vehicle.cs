using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentalVehicleService.Models
{
    public class Vehicle
    {
        [Key]
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "Tên dòng xe không được để trống")]
        public string VehicleModel { get; set; }

        [Required]
        public double Price { get; set; }

        [Range(0, 100, ErrorMessage = "Pin phải từ 0% đến 100%")]
        public int BatteryPercentage {  get; set; }

        public string State { get; set; } // Ví dụ: "Sẵn sàng", "Đang thuê", "Hỏng", "Đang bảo trì"

        public int? CurrentStationId { get; set; }

        public bool IsPluggedIn { get; set; }

        //Chỉ dùng để kiểm tra, không lưu Database
        public bool IsReadyForRent => BatteryPercentage > 15 && !IsPluggedIn;
    }
}
