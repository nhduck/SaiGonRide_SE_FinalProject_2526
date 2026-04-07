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

            var stations = await stationsQuery.Take(6).ToListAsync();

            // Stats for hero
            ViewBag.TotalStations = await _context.Stations.CountAsync(s => s.IsActive);
            ViewBag.TotalVehicles = await _context.Vehicles.CountAsync();
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
                .Take(5)
                .ToListAsync();

            return Json(stations);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Guide()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult FAQ()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
