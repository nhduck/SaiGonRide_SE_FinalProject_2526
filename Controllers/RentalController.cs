using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RentalVehicleService.Data;
using RentalVehicleService.Models;
using RentalVehicleService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RentalVehicleService.Controllers
{
    public class RentalController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly RentalService _rentalService;

        public RentalController(ApplicationDbContext context, RentalService rentalService)
        {
            _context = context; 
            _rentalService = rentalService;
        }

        // Removed redundant constructor that caused CS8618

        // GET: Rental
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Rentals.Include(r => r.EndStation).Include(r => r.StartStation).Include(r => r.Vehicle);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Rental/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rental = await _context.Rentals
                .Include(r => r.EndStation)
                .Include(r => r.StartStation)
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(m => m.RentalId == id);
            if (rental == null)
            {
                return NotFound();
            }

            return View(rental);
        }

        // GET: Rental/Create
        public IActionResult Create()
        {
            ViewData["EndStationId"] = new SelectList(_context.Stations, "StationId", "Address");
            ViewData["StartStationId"] = new SelectList(_context.Stations, "StationId", "Address");
            ViewData["VehicleId"] = new SelectList(_context.Vehicles, "VehicleId", "VehicleModel");
            return View();
        }

        // POST: Rental/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(int vehicleId, int startStationId)
        {
            var vehicle = await _context.Vehicles.FindAsync(vehicleId);
            if (vehicle == null || vehicle.State != VehicleState.Available)
            {
                TempData["Error"] = "Xe không sẵn sàng hoặc không tồn tại.";
                return RedirectToAction("Index", "Vehicles");
            }

            if (!vehicle.IsReadyForRent)
            {
                TempData["Error"] = "Xe không đủ pin để thực hiện chuyến đi (>20%).";
                return RedirectToAction("Details", "Vehicles", new { id = vehicleId });
            }

            var rental = new Rental
            {
                VehicleId = vehicleId,
                StartStationId = startStationId,
                StartTime = DateTime.Now,
                Status = RentalStatus.Active,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
                VehicleType = vehicle.Type
            };

            try
            {
                vehicle.State = VehicleState.Rented;

                var station = await _context.Stations.FindAsync(startStationId);
                if (station != null) station.CurrentCount -= 1;

                _context.Add(rental);
                await _context.SaveChangesAsync();

                return RedirectToAction("Details", new { id = rental.RentalId });
            }
            catch (Exception)
            {
                return View();
            }
        }

        // GET: Rental/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rental = await _context.Rentals.FindAsync(id);
            if (rental == null)
            {
                return NotFound();
            }
            ViewData["EndStationId"] = new SelectList(_context.Stations, "StationId", "Address", rental.EndStationId);
            ViewData["StartStationId"] = new SelectList(_context.Stations, "StationId", "Address", rental.StartStationId);
            ViewData["VehicleId"] = new SelectList(_context.Vehicles, "VehicleId", "VehicleModel", rental.VehicleId);
            return View(rental);
        }

        // POST: Rental/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int endStationId)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental == null) return NotFound();

            try
            {
                rental.EndTime = DateTime.Now;
                rental.EndStationId = endStationId;
                rental.Status = RentalStatus.Completed;

                rental.FinalFare = _rentalService.ProcessFinalBill(rental.RentalId, endStationId);

                var vehicle = await _context.Vehicles.FindAsync(rental.VehicleId);
                if (vehicle != null) vehicle.State = VehicleState.Available;

                var station = await _context.Stations.FindAsync(endStationId);
                if (station != null) station.CurrentCount += 1;

                _context.Update(rental);
                await _context.SaveChangesAsync();

                return RedirectToAction("Payment", new { id = rental.RentalId });
            }
            catch (Exception)
            {
                return View(rental);
            }
        }

        // GET: Rental/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rental = await _context.Rentals
                .Include(r => r.EndStation)
                .Include(r => r.StartStation)
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(m => m.RentalId == id);
            if (rental == null)
            {
                return NotFound();
            }

            return View(rental);
        }

        // POST: Rental/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental != null)
            {
                _context.Rentals.Remove(rental);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RentalExists(int id)
        {
            return _context.Rentals.Any(e => e.RentalId == id);
        }
    }
}
