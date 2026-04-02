namespace RentalVehicleService.Models.ViewModels
{
    public class DashboardViewModel
    {
        // Stat Cards
        public int TotalStations { get; set; }
        public int TotalVehicles { get; set; }
        public int ActiveRentals { get; set; }
        public decimal TodayRevenue { get; set; }

        // Vehicle breakdowns
        public int AvailableVehicles { get; set; }
        public int RentedVehicles { get; set; }
        public int ChargingVehicles { get; set; }
        public int MaintenanceVehicles { get; set; }
        public int StandardVehicles { get; set; }
        public int ElectricVehicles { get; set; }

        // Average fill rate
        public double AverageFillRate { get; set; }

        // Revenue by day (7 days) — for line chart
        public List<DailyRevenueItem> RevenueByDay { get; set; } = new();

        // Top 5 low fill stations
        public List<StationFillItem> LowFillStations { get; set; } = new();

        // Recent activities
        public List<RecentActivityItem> RecentActivities { get; set; } = new();
    }

    public class DailyRevenueItem
    {
        public string DayLabel { get; set; } = "";
        public decimal StandardRevenue { get; set; }
        public decimal ElectricRevenue { get; set; }
    }

    public class StationFillItem
    {
        public int StationId { get; set; }
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";
        public int CurrentCount { get; set; }
        public int TotalCapacity { get; set; }
        public double FillRate { get; set; }
    }

    public class RecentActivityItem
    {
        public int RentalId { get; set; }
        public string? UserName { get; set; }
        public string? VehicleType { get; set; }
        public string? StationName { get; set; }
        public DateTime StartTime { get; set; }
        public string? Status { get; set; }
    }
}
