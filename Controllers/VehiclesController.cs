using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalVehicleService.Data;
using RentalVehicleService.Models;

namespace RentalVehicleService.Controllers
{
    public class VehiclesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string ViewPath = "~/Views/AdminDashboard/Pages/Vehicle/";

        public VehiclesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Vehicles - Load vào AdminDashboard dưới dạng PartialView
        public async Task<IActionResult> Index()
        {
            var vehicles = await _context.Vehicles.ToListAsync();

            ViewBag.TotalVehicles = vehicles.Count;
            ViewBag.Available = vehicles.Count(v => v.State == VehicleState.Available);
            ViewBag.Charging = vehicles.Count(v => v.State == VehicleState.Charging);
            ViewBag.Maintenance = vehicles.Count(v => v.State == VehicleState.Maintenance);
            ViewBag.Rented = vehicles.Count(v => v.State == VehicleState.Rented);

            return PartialView($"{ViewPath}Index.cshtml", vehicles);
        }

        // ─────────────────────────────────────────────
        // DETAILS — trả về PartialView để load vào modal
        // ─────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var vehicle = await _context.Vehicles
                .Include(v => v.CurrentStation)   // load navigation để hiển thị tên station
                .FirstOrDefaultAsync(v => v.VehicleId == id);

            if (vehicle == null) return NotFound();

            return PartialView($"{ViewPath}_DetailsBody.cshtml", vehicle);
        }

        // ─────────────────────────────────────────────
        // CREATE
        // ─────────────────────────────────────────────
        public IActionResult Create()
        {
            return View($"{ViewPath}Create.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("VehicleModel,Price,BatteryPercentage,State,Type,LastMaintenance,CurrentStationId")] Vehicle vehicle)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vehicle);
                await _context.SaveChangesAsync();
                return Ok(new { success = true });
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(new { success = false, errors });
        }

        // ─────────────────────────────────────────────
        // EDIT GET — trả về JSON data để điền vào form
        // ─────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return NotFound();

            // Trả JSON để JS điền vào form trong modal
            return Ok(new
            {
                vehicleId = vehicle.VehicleId,
                vehicleModel = vehicle.VehicleModel,
                price = vehicle.Price,
                batteryPercentage = vehicle.BatteryPercentage,
                state = (int)vehicle.State,
                type = (int)vehicle.Type,
                lastMaintenance = vehicle.LastMaintenance.ToString("yyyy-MM-ddTHH:mm"),
                currentStationId = vehicle.CurrentStationId
            });
        }

        // ─────────────────────────────────────────────
        // EDIT POST — nhận form data từ AJAX
        // ─────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("VehicleId,VehicleModel,Price,BatteryPercentage,State,Type,LastMaintenance,CurrentStationId")] Vehicle vehicle)
        {
            if (id != vehicle.VehicleId) return NotFound();

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { success = false, errors });
            }

            try
            {
                _context.Update(vehicle);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VehicleExists(vehicle.VehicleId)) return NotFound();
                throw;
            }

            // Trả về row HTML mới để JS cập nhật bảng không reload trang
            var updated = await _context.Vehicles
                .Include(v => v.CurrentStation)
                .FirstOrDefaultAsync(v => v.VehicleId == id);

            return Ok(new { success = true });
        }

        // ─────────────────────────────────────────────
        // DELETE
        // ─────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle != null)
            {
                _context.Vehicles.Remove(vehicle);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

        // ─────────────────────────────────────────────
        // STATS
        // ─────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAmountInfo()
        {
            var vehicles = await _context.Vehicles.ToListAsync();

            return Ok(new
            {
                totalVehicles = vehicles.Count,
                available = vehicles.Count(v => v.State == VehicleState.Available),
                charging = vehicles.Count(v => v.State == VehicleState.Charging),
                maintenance = vehicles.Count(v => v.State == VehicleState.Maintenance),
                rented = vehicles.Count(v => v.State == VehicleState.Rented)
            });
        }

        private bool VehicleExists(int id) =>
            _context.Vehicles.Any(e => e.VehicleId == id);
    }
}
