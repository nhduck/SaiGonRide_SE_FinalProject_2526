using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RentalVehicleService.Data;
using Microsoft.AspNetCore.Authorization;
using RentalVehicleService.Models;

namespace RentalVehicleService.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        // Configure the view path for partial views
        private const string ViewPath = "~/Views/AdminDashboard/Pages/Station/";

        public StationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Stations
        public async Task<IActionResult> Index()
        {
            var stations = await _context.Stations.ToListAsync();
            
            // Optimized sync: Get all counts in one query
            var vehicleCounts = await _context.Vehicles
                .Where(v => v.CurrentStationId != null)
                .GroupBy(v => v.CurrentStationId)
                .Select(g => new { StationId = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var s in stations)
            {
                s.CurrentCount = vehicleCounts.FirstOrDefault(c => c.StationId == s.StationId)?.Count ?? 0;
            }
            await _context.SaveChangesAsync();

            ViewBag.TotalStations = stations.Count;
            ViewBag.Active = stations.Count(s => s.IsActive);
            ViewBag.LowStock = stations.Count(s => s.IsLowInventory);
            ViewBag.Inactive = stations.Count(s => !s.IsActive);
            ViewBag.TotalActive = stations.Count(s => s.IsActive);

            // Return PartialView with specific path
            return PartialView($"{ViewPath}Index.cshtml", stations);
        }

        // GET: Stations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var station = await _context.Stations.FirstOrDefaultAsync(m => m.StationId == id);
            if (station == null) return NotFound();

            return PartialView($"{ViewPath}Details.cshtml", station);
        }

        // GET: Stations/Create
        public IActionResult Create()
        {
            return PartialView($"{ViewPath}Create.cshtml");
        }

        // POST: Stations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StationId,Name,Address,Latitude,Longitude,TotalCapacity,CurrentCount,IsActive")] Station station)
        {
            if (station.CurrentCount > station.TotalCapacity)
            {
                ModelState.AddModelError("CurrentCount", "Error: Current vehicle count cannot exceed total capacity!");
            }

            if (ModelState.IsValid)
            {
                _context.Add(station);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return PartialView($"{ViewPath}Create.cshtml", station);
        }

        // GET: Stations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var station = await _context.Stations.FindAsync(id);
            if (station == null) return NotFound();

            return PartialView($"{ViewPath}Edit.cshtml", station);
        }

        // POST: Stations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("StationId,Name,Address,Latitude,Longitude,TotalCapacity,CurrentCount,IsActive")] Station station)
        {
            if (id != station.StationId) return NotFound();

            if (station.CurrentCount > station.TotalCapacity)
            {
                ModelState.AddModelError("CurrentCount", "Error: Current vehicle count cannot exceed total capacity!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(station);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StationExists(station.StationId)) return NotFound();
                    else throw;
                }
                return Json(new { success = true });
            }
            return PartialView($"{ViewPath}Edit.cshtml", station);
        }

        // POST: Stations/DeleteConfirmed
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var station = await _context.Stations.FindAsync(id);

            if (station == null)
                return NotFound(new { message = "Station not found." });

            // Check vehicles linked via FK in Vehicles table
            var vehicleCountInDb = await _context.Vehicles.CountAsync(v => v.CurrentStationId == id);

            var effectiveVehicleCount = Math.Max(vehicleCountInDb, station.CurrentCount);

            if (effectiveVehicleCount > 0)
            {
                return BadRequest(new { message = $"Cannot delete station \"{station.Name}\" because it still has {effectiveVehicleCount} vehicle(s) assigned. Please reassign or remove them first." });
            }

            // Check rentals referencing this station
            var rentalCount = await _context.Rentals.CountAsync(r => r.StartStationId == id || r.EndStationId == id);
            if (rentalCount > 0)
            {
                return BadRequest(new { message = $"Cannot delete station \"{station.Name}\" because it is referenced by {rentalCount} rental record(s)." });
            }

            try
            {
                _context.Stations.Remove(station);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Station deleted successfully." });
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new { message = "Cannot delete this station because it has related data. Please remove all associated records first." });
            }
        }

        // GET: Stations/GetAmountInfo
        public async Task<IActionResult> GetAmountInfo()
        {
            var stations = await _context.Stations.ToListAsync();
            var vehicleCounts = await _context.Vehicles
                .Where(v => v.CurrentStationId != null)
                .GroupBy(v => v.CurrentStationId)
                .Select(g => new { StationId = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var s in stations)
            {
                s.CurrentCount = vehicleCounts.FirstOrDefault(c => c.StationId == s.StationId)?.Count ?? 0;
            }
            await _context.SaveChangesAsync();

            return Ok(new
            {
                totalStations = stations.Count,
                active = stations.Count(s => s.IsActive),
                inactive = stations.Count(s => !s.IsActive),
                lowStock = stations.Count(s => s.IsLowInventory),
                totalCapacity = stations.Sum(s => s.TotalCapacity),
                totalVehicles = stations.Sum(s => s.CurrentCount)
            });
        }

        // GET: Stations/SearchStations
        public async Task<IActionResult> SearchStations(string searchTerm, List<string> statuses)
        {
            var query = _context.Stations.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                string st = searchTerm.ToLower();
                query = query.Where(s =>
                    s.StationId.ToString().Contains(st) ||
                    s.Name.ToLower().Contains(st) ||
                    s.Address.ToLower().Contains(st)
                );
            }

            var stations = await query.ToListAsync();

            var vehicleCounts = await _context.Vehicles
                .Where(v => v.CurrentStationId != null)
                .GroupBy(v => v.CurrentStationId)
                .Select(g => new { StationId = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var s in stations)
            {
                s.CurrentCount = vehicleCounts.FirstOrDefault(c => c.StationId == s.StationId)?.Count ?? 0;
            }
            await _context.SaveChangesAsync();

            if (statuses != null && statuses.Any())
            {
                stations = stations.Where(s =>
                {
                    bool match = false;
                    if (statuses.Contains("Active") && s.IsActive && !s.IsLowInventory && s.FillRate <= 0.85) match = true;
                    if (statuses.Contains("Low Stock") && s.IsLowInventory) match = true;
                    if (statuses.Contains("Full") && s.FillRate > 0.85) match = true;
                    if (statuses.Contains("Inactive") && !s.IsActive) match = true;
                    return match;
                }).ToList();
            }

            return PartialView($"{ViewPath}_StationTablePartial.cshtml", stations);
        }

        private bool StationExists(int id)
        {
            return _context.Stations.Any(e => e.StationId == id);
        }
    }
}