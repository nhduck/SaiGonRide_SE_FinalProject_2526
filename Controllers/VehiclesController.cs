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
            var stations = await _context.Stations
                .Where(s => s.IsActive)
                .OrderBy(s => s.StationId)
                .ToListAsync();

            ViewBag.Stations = stations;
            ViewBag.TotalVehicles = vehicles.Count;
            ViewBag.Available = vehicles.Count(v => v.State == VehicleState.Available);
            ViewBag.UnAvailable = vehicles.Count(v => v.State == VehicleState.UnAvailable);
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
        public async Task<IActionResult> Edit(int id, [Bind("VehicleId,VehicleModel,Price,BatteryPercentage,State,Type,LastMaintenance,CurrentStationId")] Vehicle vehicle)
        {
            if (id != vehicle.VehicleId) return BadRequest();

            // 1. Xử lý DateTime: Nếu ngày bị mặc định là MinValue, hãy gán ngày hiện tại
            if (vehicle.LastMaintenance == DateTime.MinValue)
            {
                vehicle.LastMaintenance = DateTime.Now;
            }

            // 2. Kiểm tra ModelState
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { success = false, errors });
            }

            try
            {
                _context.Update(vehicle);
                await _context.SaveChangesAsync();
                return Ok(new { success = true });
            }
            catch (DbUpdateException)
            {
                return BadRequest(new { success = false, errors = new[] { "Lỗi Foreign Key: Station ID không tồn tại." } });
            }
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
                unAvailable = vehicles.Count(v => v.State == VehicleState.UnAvailable),
                charging = vehicles.Count(v => v.State == VehicleState.Charging),
                maintenance = vehicles.Count(v => v.State == VehicleState.Maintenance),
                rented = vehicles.Count(v => v.State == VehicleState.Rented)
            });
        }

        private bool VehicleExists(int id) =>
            _context.Vehicles.Any(e => e.VehicleId == id);
    }
}
