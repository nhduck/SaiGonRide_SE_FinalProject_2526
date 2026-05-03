using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalVehicleService.Data;
using RentalVehicleService.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace RentalVehicleService.Controllers
{
    public class AdminDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var totalVehicles = await _context.Vehicles.CountAsync();
            var activeRentals = await _context.Rentals.CountAsync(r => r.Status == RentalStatus.Active);
            var totalUsers = await _context.Users.CountAsync();
            var totalRevenue = await _context.Rentals
                .Where(r => r.Status == RentalStatus.Completed)
                .SumAsync(r => r.FinalFare);

            // Get revenue for last 7 days for a chart
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.Today.AddDays(-i))
                .OrderBy(d => d)
                .ToList();

            var revenueData = new List<decimal>();
            foreach (var day in last7Days)
            {
                var dayRevenue = await _context.Rentals
                    .Where(r => r.Status == RentalStatus.Completed && r.StartTime.Date == day.Date)
                    .SumAsync(r => r.FinalFare);
                revenueData.Add(dayRevenue);
            }

            ViewBag.TotalVehicles = totalVehicles;
            ViewBag.ActiveRentals = activeRentals;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.RevenueLabels = last7Days.Select(d => d.ToString("dd/MM")).ToList();
            ViewBag.RevenueValues = revenueData;

            // Top vehicles by rental count
            var topVehicles = await _context.Rentals
                .GroupBy(r => r.VehicleId)
                .Select(g => new { VehicleId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .Join(_context.Vehicles, x => x.VehicleId, v => v.VehicleId, (x, v) => new { v.VehicleModel, x.Count })
                .ToListAsync();

            ViewBag.TopVehicles = topVehicles;

            // Low battery vehicles
            var lowBatteryVehicles = await _context.Vehicles
                .Where(v => v.BatteryPercentage < 20)
                .OrderBy(v => v.BatteryPercentage)
                .Take(5)
                .ToListAsync();
            
            ViewBag.LowBatteryVehicles = lowBatteryVehicles;
            
            // Recent Activity (Mix of recent rentals and new users)
            var recentRentals = await _context.Rentals
                .Include(r => r.Vehicle)
                .OrderByDescending(r => r.StartTime)
                .Take(5)
                .Select(r => new {
                    Type = "Rental",
                    TitleKey = r.Status == RentalStatus.Active ? "admin_activity_rental_started" : "admin_activity_rental_completed",
                    DescriptionKey = r.Status == RentalStatus.Active ? "admin_activity_rental_started_desc" : "admin_activity_rental_completed_desc",
                    Param1 = r.UserId.Substring(0, Math.Min(5, r.UserId.Length)) + "...",
                    Param2 = (r.Vehicle != null ? r.Vehicle.VehicleModel : "Unknown"),
                    Time = r.StartTime,
                    IconClass = r.Status == RentalStatus.Active ? "bg-primary" : "bg-success",
                    Icon = r.Status == RentalStatus.Active ? "bi-bicycle" : "bi-check-lg"
                })
                .ToListAsync();

            var recentUsers = await _context.Users
                .OrderByDescending(u => u.Id)
                .Take(3)
                .Select(u => new {
                    Type = "User",
                    TitleKey = "admin_activity_new_user",
                    DescriptionKey = "admin_activity_new_user_desc",
                    Param1 = u.UserName,
                    Param2 = "",
                    Time = DateTime.Now.AddHours(-1),
                    IconClass = "bg-warning",
                    Icon = "bi-person-plus"
                })
                .ToListAsync();

            ViewBag.RecentActivities = recentRentals.Cast<dynamic>()
                .Concat(recentUsers.Cast<dynamic>())
                .OrderByDescending(a => a.Time)
                .Take(6)
                .ToList();

            return PartialView("~/Views/AdminDashboard/Pages/Dashboard/Index.cshtml");
        }

        public IActionResult SearchVehicles(string searchTerm, List<string> statuses)
        {
            var query = _context.Vehicles.Include(v => v.CurrentStation).AsQueryable();

            var allVehicles = query.ToList();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                string st = searchTerm.ToLower();
                allVehicles = allVehicles.Where(v =>
                    v.VehicleId.ToString().Contains(st) ||
                    v.VehicleModel.ToLower().Contains(st) ||
                    v.Type.ToString().ToLower().Contains(st)
                    ).ToList();
            }

            if (statuses != null && statuses.Any())
            {
                allVehicles = allVehicles.Where(v => statuses.Contains(v.State.ToString())).ToList();
            }

            return PartialView("~/Views/AdminDashboard/Pages/Vehicle/_VehicleTablePartial.cshtml", allVehicles);
        }
    }
}