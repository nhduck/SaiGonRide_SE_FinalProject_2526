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

        public IActionResult Index()
        {
            var vehicles = _context.Vehicles.ToList();
            return View(vehicles);
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
                    v.VehicleModel.ToLower().Contains(st) ||
                    v.Type.ToString().ToLower().Contains(st)
                ).ToList();
            }

            if (statuses != null && statuses.Any())
            {
                allVehicles = allVehicles.Where(v => statuses.Contains(v.State.ToString())).ToList();
            }

            return PartialView("~/Views/AdminDashboard/Pages/Vehicle/_VehicleTablePartial.cshtml", allVehicles);
        }
    }
}