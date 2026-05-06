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
            var recentRentalsQuery = await (from r in _context.Rentals
                                       join u in _context.Users on r.UserId equals u.Id
                                       join v in _context.Vehicles on r.VehicleId equals v.VehicleId into vGroup
                                       from v in vGroup.DefaultIfEmpty()
                                       orderby r.StartTime descending
                                       select new {
                                           r.Status,
                                           VehicleModel = v != null ? v.VehicleModel : "Unknown",
                                           UserName = u.UserName,
                                           Time = r.StartTime
                                       })
                                       .Take(5)
                                       .ToListAsync();

            var recentRentals = recentRentalsQuery.Select(r => new {
                    Type = "Rental",
                    TitleKey = r.Status == RentalStatus.Active ? "admin_activity_rental_started" : "admin_activity_rental_completed",
                    DescriptionKey = r.Status == RentalStatus.Active ? "admin_activity_rental_started_desc" : "admin_activity_rental_completed_desc",
                    Param1 = r.UserName,
                    Param2 = r.VehicleModel,
                    Time = r.Time,
                    IconClass = r.Status == RentalStatus.Active ? "bg-primary" : "bg-success",
                    Icon = r.Status == RentalStatus.Active ? "bi-bicycle" : "bi-check-lg"
                }).ToList();

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
                    (v.VehicleModel != null && v.VehicleModel.ToLower().Contains(st)) ||
                    v.Type.ToString().ToLower().Contains(st)
                    ).ToList();
            }

            if (statuses != null && statuses.Any())
            {
                allVehicles = allVehicles.Where(v => statuses.Contains(v.State.ToString())).ToList();
            }

            return PartialView("~/Views/AdminDashboard/Pages/Vehicle/_VehicleTablePartial.cshtml", allVehicles);
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
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

            var activities = recentRentals.Cast<dynamic>()
                .Concat(recentUsers.Cast<dynamic>())
                .OrderByDescending(a => a.Time)
                .Take(6)
                .ToList();

            return Json(activities);
        }

        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            var totalVehicles = await _context.Vehicles.CountAsync();
            var activeRentals = await _context.Rentals.CountAsync(r => r.Status == RentalStatus.Active);
            var totalUsers = await _context.Users.CountAsync();
            var totalRevenue = await _context.Rentals
                .Where(r => r.Status == RentalStatus.Completed)
                .SumAsync(r => r.FinalFare);

            return Json(new
            {
                totalVehicles,
                activeRentals,
                totalUsers,
                totalRevenue = totalRevenue.ToString("N0")
            });
        }

        // ─── Station Inventory Status ─────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetStationInventoryStatus()
        {
            var stations = await _context.Stations
                .Where(s => s.IsActive)
                .ToListAsync();

            var overfull = stations
                .Where(s => s.TotalCapacity > 0 && (double)s.CurrentCount / s.TotalCapacity >= 0.90)
                .Select(s => new {
                    s.StationId,
                    s.Name,
                    s.Address,
                    s.CurrentCount,
                    s.TotalCapacity,
                    FillRate = Math.Round((double)s.CurrentCount / s.TotalCapacity * 100, 1)
                })
                .OrderByDescending(s => s.FillRate)
                .ToList();

            var shortage = stations
                .Where(s => s.TotalCapacity > 0 && (double)s.CurrentCount / s.TotalCapacity < 0.20)
                .Select(s => new {
                    s.StationId,
                    s.Name,
                    s.Address,
                    s.CurrentCount,
                    s.TotalCapacity,
                    FillRate = Math.Round((double)s.CurrentCount / s.TotalCapacity * 100, 1)
                })
                .OrderBy(s => s.FillRate)
                .ToList();

            return Json(new { overfull, shortage });
        }

        // ─── CSV Export Endpoints ─────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> ExportDashboardCsv()
        {
            var totalVehicles = await _context.Vehicles.CountAsync();
            var activeRentals = await _context.Rentals.CountAsync(r => r.Status == RentalStatus.Active);
            var totalUsers = await _context.Users.CountAsync();
            var totalRevenue = await _context.Rentals
                .Where(r => r.Status == RentalStatus.Completed)
                .SumAsync(r => r.FinalFare);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Metric,Value");
            sb.AppendLine($"Export Date,{DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            sb.AppendLine($"Total Vehicles,{totalVehicles}");
            sb.AppendLine($"Active Rentals,{activeRentals}");
            sb.AppendLine($"Total Users,{totalUsers}");
            sb.AppendLine($"Total Revenue (VND),{totalRevenue:N0}");

            var dateStr = DateTime.Now.ToString("yyyy-MM-dd");
            return File(Utf8BomBytes(sb.ToString()), "text/csv; charset=utf-8", $"dashboard-report-{dateStr}.csv");
        }

        [HttpGet]
        public async Task<IActionResult> ExportRevenueCsv()
        {
            var rentals = await _context.Rentals
                .Include(r => r.Vehicle)
                .Include(r => r.StartStation)
                .Include(r => r.EndStation)
                .Where(r => r.Status == RentalStatus.Completed)
                .OrderByDescending(r => r.EndTime)
                .ToListAsync();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Rental ID,User ID,Vehicle Model,Vehicle Type,Start Station,End Station,Start Time,End Time,Duration (min),Final Fare (VND),Discount (VND)");

            foreach (var r in rentals)
            {
                var duration = r.EndTime.HasValue
                    ? Math.Round((r.EndTime.Value - r.StartTime).TotalMinutes, 1)
                    : 0;
                var model = EscapeCsv(r.Vehicle?.VehicleModel ?? "N/A");
                var startStation = EscapeCsv(r.StartStation?.Name ?? "N/A");
                var endStation = EscapeCsv(r.EndStation?.Name ?? "N/A");
                sb.AppendLine($"{r.RentalId},{EscapeCsv(r.UserId)},{model},{r.VehicleType},{startStation},{endStation},{r.StartTime:dd/MM/yyyy HH:mm},{r.EndTime:dd/MM/yyyy HH:mm},{duration},{r.FinalFare:N0},{r.DiscountAmount?.ToString("N0") ?? "0"}");
            }

            var dateStr = DateTime.Now.ToString("yyyy-MM-dd");
            return File(Utf8BomBytes(sb.ToString()), "text/csv; charset=utf-8", $"revenue-report-{dateStr}.csv");
        }

        [HttpGet]
        public async Task<IActionResult> ExportStationInventoryCsv()
        {
            var stations = await _context.Stations
                .Include(s => s.Vehicles)
                .OrderBy(s => s.Name)
                .ToListAsync();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Station ID,Station Name,Address,Total Capacity,Current Count,Fill Rate (%),Status,Active Vehicles,Charging Vehicles,Maintenance Vehicles");

            foreach (var s in stations)
            {
                var fillRate = s.TotalCapacity > 0 ? Math.Round((double)s.CurrentCount / s.TotalCapacity * 100, 1) : 0;
                var status = fillRate >= 90 ? "Overfull" : fillRate < 20 ? "Shortage" : "Normal";
                var activeVehicles = s.Vehicles.Count(v => v.State == VehicleState.Available);
                var chargingVehicles = s.Vehicles.Count(v => v.State == VehicleState.Charging);
                var maintenanceVehicles = s.Vehicles.Count(v => v.State == VehicleState.Maintenance);

                sb.AppendLine($"{s.StationId},{EscapeCsv(s.Name)},{EscapeCsv(s.Address)},{s.TotalCapacity},{s.CurrentCount},{fillRate},{status},{activeVehicles},{chargingVehicles},{maintenanceVehicles}");
            }

            var dateStr = DateTime.Now.ToString("yyyy-MM-dd");
            return File(Utf8BomBytes(sb.ToString()), "text/csv; charset=utf-8", $"station-inventory-report-{dateStr}.csv");
        }

        // Trả về bytes với UTF-8 BOM để Excel đọc tiếng Việt đúng
        private static byte[] Utf8BomBytes(string content)
        {
            var bom = new byte[] { 0xEF, 0xBB, 0xBF };
            var body = System.Text.Encoding.UTF8.GetBytes(content);
            var result = new byte[bom.Length + body.Length];
            bom.CopyTo(result, 0);
            body.CopyTo(result, bom.Length);
            return result;
        }

        private static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
        }

        // ─── Reports — load vào AdminDashboard qua AJAX loadPage() ───────────
        [HttpGet]
        public async Task<IActionResult> Reports()
        {
            var reports = await _context.BugReport
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();

            ViewBag.TotalReports = reports.Count;
            ViewBag.NewReports = reports.Count(r => r.Status == BugReport.BugStatus.New);
            ViewBag.InProgress = reports.Count(r => r.Status == BugReport.BugStatus.InProgress);
            ViewBag.Resolved = reports.Count(r => r.Status == BugReport.BugStatus.Resolved);
            ViewBag.Closed = reports.Count(r => r.Status == BugReport.BugStatus.Closed);

            return PartialView("~/Views/AdminDashboard/Pages/BugReports/Index.cshtml", reports);
        }

        [HttpGet]
        public async Task<IActionResult> Notifications()
        {
            var recentRentals = await _context.Rentals
                .Include(r => r.Vehicle)
                .OrderByDescending(r => r.StartTime)
                .Take(20)
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
                .Take(10)
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

            ViewBag.AllActivities = recentRentals.Cast<dynamic>()
                .Concat(recentUsers.Cast<dynamic>())
                .OrderByDescending(a => a.Time)
                .ToList();

            return PartialView("~/Views/AdminDashboard/Pages/Notifications/Index.cshtml");
        }
    }
}