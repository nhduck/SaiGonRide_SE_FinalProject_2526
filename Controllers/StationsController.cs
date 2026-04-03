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
        // Thêm dòng cấu hình đường dẫn ViewPath
        private const string ViewPath = "~/Views/AdminDashboard/Pages/StationManagement/";

        public StationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Stations
        public async Task<IActionResult> Index()
        {
            var stations = await _context.Stations.ToListAsync();
            // Trả về PartialView theo đường dẫn cụ thể
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
        public async Task<IActionResult> Create([Bind("StationId,Name,Address,TotalCapacity,CurrentCount,IsActive")] Station station)
        {
            if (station.CurrentCount > station.TotalCapacity)
            {
                ModelState.AddModelError("CurrentCount", "Lỗi: Số xe hiện tại không thể lớn hơn sức chứa tối đa!");
            }

            if (ModelState.IsValid)
            {
                _context.Add(station);
                await _context.SaveChangesAsync();
                // Sau khi tạo xong, thường sẽ load lại danh sách Index
                return RedirectToAction(nameof(Index));
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
        public async Task<IActionResult> Edit(int id, [Bind("StationId,Name,Address,TotalCapacity,CurrentCount,IsActive")] Station station)
        {
            if (id != station.StationId) return NotFound();

            if (station.CurrentCount > station.TotalCapacity)
            {
                ModelState.AddModelError("CurrentCount", "Lỗi: Số xe hiện tại không thể lớn hơn sức chứa tối đa!");
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
                return RedirectToAction(nameof(Index));
            }
            return PartialView($"{ViewPath}Edit.cshtml", station);
        }

        // GET: Stations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var station = await _context.Stations.FirstOrDefaultAsync(m => m.StationId == id);
            if (station == null) return NotFound();

            return PartialView($"{ViewPath}Delete.cshtml", station);
        }

        // POST: Stations/Delete/5
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
            return RedirectToAction(nameof(Index));
        }

        private bool StationExists(int id)
        {
            return _context.Stations.Any(e => e.StationId == id);
        }
    }
}