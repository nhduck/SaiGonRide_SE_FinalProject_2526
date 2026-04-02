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

        public StationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Stations
        public async Task<IActionResult> Index()
        {
            return View(await _context.Stations.ToListAsync());
        }

        // GET: Stations/Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var station = await _context.Stations
                .FirstOrDefaultAsync(m => m.StationId == id);
            if (station == null)
            {
                return NotFound();
            }

            return View(station);
        }

        // GET: Stations/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Stations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StationId,Name,Address,Latitude,Longitude,TotalCapacity,CurrentCount,IsActive")] Station station)
        {
            // Không cho phép số xe hiện tại lớn hơn sức chứa
            if (station.CurrentCount > station.TotalCapacity)
            {
                ModelState.AddModelError("CurrentCount", "Lỗi: Số xe hiện tại không thể lớn hơn sức chứa tối đa của trạm!");
            }
            if (ModelState.IsValid)
            {
                _context.Add(station);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Station created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(station);
        }

        // GET: Stations/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var station = await _context.Stations.FindAsync(id);
            if (station == null)
            {
                return NotFound();
            }

            if (station.CurrentCount > station.TotalCapacity)
            {
                ModelState.AddModelError("CurrentCount", "Lỗi: Số xe hiện tại không thể lớn hơn sức chứa tối đa của trạm vừa tạo!");
            }

            return View(station);
        }

        // POST: Stations/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("StationId,Name,Address,Latitude,Longitude,TotalCapacity,CurrentCount,IsActive")] Station station)
        {
            if (id != station.StationId)
            {
                return NotFound();
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
                    if (!StationExists(station.StationId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["SuccessMessage"] = "Station updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(station);
        }

        // GET: Stations/Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var station = await _context.Stations
                .FirstOrDefaultAsync(m => m.StationId == id);
            if (station == null)
            {
                return NotFound();
            }

            return View(station);
        }

        // POST: Stations/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var station = await _context.Stations.FindAsync(id);
            if (station != null)
            {
                _context.Stations.Remove(station);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Station deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private bool StationExists(int id)
        {
            return _context.Stations.Any(e => e.StationId == id);
        }
    }
}
