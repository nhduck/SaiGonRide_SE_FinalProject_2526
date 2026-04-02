using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalVehicleService.Data;
using RentalVehicleService.Models;
using RentalVehicleService.Models.ViewModels;

namespace RentalVehicleService.Controllers.Admin
{
    [Route("Admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var vm = await BuildDashboardViewModel();
            return View(vm);
        }

        [HttpGet("GetStats")]
        public async Task<IActionResult> GetStats()
        {
            var vm = await BuildDashboardViewModel();
            return Json(new
            {
                totalStations = vm.TotalStations,
                totalVehicles = vm.TotalVehicles,
                activeRentals = vm.ActiveRentals,
                todayRevenue = vm.TodayRevenue,
                availableVehicles = vm.AvailableVehicles,
                rentedVehicles = vm.RentedVehicles
            });
        }

        private async Task<DashboardViewModel> BuildDashboardViewModel()
        {
            var today = DateTime.Today;
            var sevenDaysAgo = today.AddDays(-6);

            // Stations
            var stations = await _context.Stations.Where(s => s.IsActive).ToListAsync();
            var totalStations = stations.Count;

            // Vehicles
            var vehicles = await _context.Vehicles.ToListAsync();
            var totalVehicles = vehicles.Count;
            var availableVehicles = vehicles.Count(v => v.State == VehicleState.Available);
            var rentedVehicles = vehicles.Count(v => v.State == VehicleState.Rented);
            var chargingVehicles = vehicles.Count(v => v.State == VehicleState.Charging);
            var maintenanceVehicles = vehicles.Count(v => v.State == VehicleState.Maintenance);
            var standardVehicles = vehicles.Count(v => v.Type == VehicleType.Standard);
            var electricVehicles = vehicles.Count(v => v.Type == VehicleType.Electric);

            // Active rentals
            var activeRentals = await _context.Rentals.CountAsync(r => r.Status == RentalStatus.Active);

            // Today revenue
            var todayRevenue = await _context.Rentals
                .Where(r => r.Status == RentalStatus.Completed && r.EndTime.HasValue && r.EndTime.Value.Date == today)
                .SumAsync(r => r.FinalFare);

            // Average fill rate
            var avgFillRate = stations.Any() ? stations.Average(s => s.FillRate) : 0;

            // Revenue by day (7 days)
            var revenueByDay = new List<DailyRevenueItem>();
            var culture = new CultureInfo("vi-VN");
            for (int i = 0; i < 7; i++)
            {
                var day = sevenDaysAgo.AddDays(i);
                var dayRentals = await _context.Rentals
                    .Where(r => r.Status == RentalStatus.Completed && r.EndTime.HasValue && r.EndTime.Value.Date == day)
                    .ToListAsync();

                revenueByDay.Add(new DailyRevenueItem
                {
                    DayLabel = day.ToString("dd/MM"),
                    StandardRevenue = dayRentals.Where(r => r.VehicleType == VehicleType.Standard).Sum(r => r.FinalFare),
                    ElectricRevenue = dayRentals.Where(r => r.VehicleType == VehicleType.Electric).Sum(r => r.FinalFare)
                });
            }

            // Low fill stations (top 5 lowest)
            var lowFillStations = stations
                .OrderBy(s => s.FillRate)
                .Take(5)
                .Select(s => new StationFillItem
                {
                    StationId = s.StationId,
                    Name = s.Name,
                    Address = s.Address,
                    CurrentCount = s.CurrentCount,
                    TotalCapacity = s.TotalCapacity,
                    FillRate = s.FillRate
                })
                .ToList();

            // Recent activities (10 latest rentals)
            var recentRentals = await _context.Rentals
                .Include(r => r.StartStation)
                .Include(r => r.Vehicle)
                .OrderByDescending(r => r.StartTime)
                .Take(10)
                .ToListAsync();

            var recentActivities = new List<RecentActivityItem>();
            foreach (var r in recentRentals)
            {
                var user = await _userManager.FindByIdAsync(r.UserId);
                recentActivities.Add(new RecentActivityItem
                {
                    RentalId = r.RentalId,
                    UserName = user?.FullName ?? user?.Email ?? "N/A",
                    VehicleType = r.VehicleType == VehicleType.Standard ? "Standard" : "Electric",
                    StationName = r.StartStation?.Name ?? "N/A",
                    StartTime = r.StartTime,
                    Status = r.Status.ToString()
                });
            }

            return new DashboardViewModel
            {
                TotalStations = totalStations,
                TotalVehicles = totalVehicles,
                ActiveRentals = activeRentals,
                TodayRevenue = todayRevenue,
                AvailableVehicles = availableVehicles,
                RentedVehicles = rentedVehicles,
                ChargingVehicles = chargingVehicles,
                MaintenanceVehicles = maintenanceVehicles,
                StandardVehicles = standardVehicles,
                ElectricVehicles = electricVehicles,
                AverageFillRate = avgFillRate,
                RevenueByDay = revenueByDay,
                LowFillStations = lowFillStations,
                RecentActivities = recentActivities
            };
        }
    }
}
