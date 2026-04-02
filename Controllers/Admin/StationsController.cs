using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalVehicleService.Data;
using RentalVehicleService.Models;

namespace RentalVehicleService.Controllers.Admin
{
    [Route("Admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class StationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Stations
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(string? searchTerm, string? fillFilter)
        {
            var query = _context.Stations.Where(s => s.IsActive).AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(s => s.Name.Contains(searchTerm) || s.Address.Contains(searchTerm));
                ViewBag.SearchTerm = searchTerm;
            }

            var stations = await query.OrderBy(s => s.Name).ToListAsync();

            // Filter by fill status
            if (!string.IsNullOrWhiteSpace(fillFilter))
            {
                ViewBag.FillFilter = fillFilter;
                stations = fillFilter switch
                {
                    "low" => stations.Where(s => s.FillRate < 0.20).ToList(),
                    "full" => stations.Where(s => s.FillRate > 0.85).ToList(),
                    "normal" => stations.Where(s => s.FillRate >= 0.20 && s.FillRate <= 0.85).ToList(),
                    _ => stations
                };
            }

            ViewBag.TotalActive = await _context.Stations.CountAsync(s => s.IsActive);
            return View(stations);
        }

        // GET: Admin/Stations/Details/5
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var station = await _context.Stations
                .Include(s => s.Vehicles)
                .FirstOrDefaultAsync(s => s.StationId == id);

            if (station == null) return NotFound();

            return View(station);
        }

        // GET: Admin/Stations/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Stations/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Station station)
        {
            if (station.CurrentCount > station.TotalCapacity)
            {
                ModelState.AddModelError("CurrentCount", $"Current count cannot be greater than total capacity ({station.TotalCapacity} vehicles)");
            }

            if (ModelState.IsValid)
            {
                _context.Add(station);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Station \"{station.Name}\" created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(station);
        }

        // GET: Admin/Stations/Edit/5
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var station = await _context.Stations.FindAsync(id);
            if (station == null) return NotFound();
            return View(station);
        }

        // POST: Admin/Stations/Edit/5
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Station station)
        {
            if (id != station.StationId) return NotFound();

            if (station.TotalCapacity < station.CurrentCount)
            {
                ModelState.AddModelError("TotalCapacity", $"Total capacity cannot be less than current count ({station.CurrentCount} vehicles)");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(station);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Station \"{station.Name}\" updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Stations.AnyAsync(s => s.StationId == id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(station);
        }

        // POST: Admin/Stations/Delete/5 (soft delete)
        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var station = await _context.Stations
                .Include(s => s.Vehicles)
                .FirstOrDefaultAsync(s => s.StationId == id);

            if (station == null) return NotFound();

            // Check for vehicles in transit
            var inTransitCount = station.Vehicles.Count(v => v.State == VehicleState.Rented);
            if (inTransitCount > 0)
            {
                TempData["Error"] = $"Cannot disable station \"{station.Name}\" — {inTransitCount} vehicles are currently rented.";
                return RedirectToAction(nameof(Index));
            }

            station.IsActive = false;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Station \"{station.Name}\" has been disabled.";
            return RedirectToAction(nameof(Index));
        }

        // API: Admin/Stations/GetFillRate/5
        [HttpGet("GetFillRate/{id}")]
        public async Task<IActionResult> GetFillRate(int id)
        {
            var station = await _context.Stations.FindAsync(id);
            if (station == null) return NotFound();

            return Json(new
            {
                fillRate = Math.Round(station.FillRate * 100, 1),
                isLowInventory = station.IsLowInventory
            });
        }
    }
}
