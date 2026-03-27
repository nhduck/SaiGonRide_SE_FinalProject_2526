using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentalVehicleService.Models
{
    public enum VehicleState
    {
        Available, // Sẵn sàng
        Rented,    // Đang thuê
        Charging, // Đang sạc
        Maintenance // Bảo trì

    }

    public enum VehicleType
    {
        Electric,
        Standard
    }

    public class Vehicle
    {
        [Key]
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "Tên dòng xe không được để trống")]
        public string? VehicleModel { get; set; }

        [Required]
        public double Price { get; set; }

        [Range(-1, 100, ErrorMessage = "Pin phải từ 0% đến 100%")]
        public int BatteryPercentage {  get; set; }

        public VehicleState State { get; set; }

        public VehicleType Type { get; set; }

        public DateTime LastMaintenance {get; set; }
        public int? CurrentStationId { get; set; }

        //Chỉ dùng để kiểm tra, không lưu Database
        public bool IsReadyForRent => BatteryPercentage > 20;
    }
}
