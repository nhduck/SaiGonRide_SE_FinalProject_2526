using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalVehicleService.Data;
using RentalVehicleService.Models;

namespace RentalVehicleService.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index(string? query)
        {
            var stationsQuery = _context.Stations
                .Where(s => s.IsActive)
                .Include(s => s.Vehicles);

            if (!string.IsNullOrWhiteSpace(query))
            {
                stationsQuery = stationsQuery
                    .Where(s => s.Name.Contains(query) || s.Address.Contains(query))
                    .Include(s => s.Vehicles);
            }

            var stations = await stationsQuery
                .OrderByDescending(s => s.StationId)
                .ToListAsync();

            // Sync vehicle counts to ensure accuracy on Home page (Only count Available vehicles)
            var vehicleCounts = await _context.Vehicles
                .Where(v => v.CurrentStationId != null && v.State == VehicleState.Available)
                .GroupBy(v => v.CurrentStationId)
                .Select(g => new { StationId = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var s in stations)
            {
                s.CurrentCount = vehicleCounts.FirstOrDefault(c => c.StationId == s.StationId)?.Count ?? 0;
            }
            await _context.SaveChangesAsync();

            // Stats for hero
            ViewBag.TotalStations = await _context.Stations.CountAsync(s => s.IsActive);
            ViewBag.TotalVehicles = await _context.Vehicles.CountAsync();
            ViewBag.TotalRides = await _context.Rentals.CountAsync(); // Total ever
            ViewBag.ActiveRentals = await _context.Rentals.CountAsync(r => r.Status == RentalStatus.Active);
            ViewBag.SearchQuery = query;

            return View(stations);
        }

        [HttpGet]
        public async Task<IActionResult> SearchStations(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Json(new List<object>());

            var stations = await _context.Stations
                .Where(s => s.IsActive &&
                       (s.Name.Contains(query) || s.Address.Contains(query)))
                .Select(s => new
                {
                    s.StationId,
                    s.Name,
                    s.Address,
                    s.CurrentCount,
                    s.TotalCapacity
                })
                .OrderBy(s => s.StationId)
                .Take(5)
                .ToListAsync();

            return Json(stations);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Guide()
        {
            ViewBag.TotalStations = await _context.Stations.CountAsync(s => s.IsActive);
            return View();
        }

        public async Task<IActionResult> About()
        {
            ViewBag.TotalStations = await _context.Stations.CountAsync(s => s.IsActive);
            ViewBag.TotalVehicles = await _context.Vehicles.CountAsync();
            return View();
        }

        public IActionResult FAQ()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ReportIssue()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReportIssue(BugReport model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.CreatedDate = DateTime.Now;
            model.Status = BugReport.BugStatus.New;

            _context.BugReport.Add(model);
            await _context.SaveChangesAsync();

            TempData["ReportSuccess"] = true;
            return RedirectToAction(nameof(ReportIssue));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
