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
            var query = _context.Vehicles.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                string st = searchTerm.ToLower();
                query = query.Where(v =>
                    v.VehicleId.ToString().Contains(st) ||
                    v.VehicleModel.ToLower().Contains(st)
                );
            }

            var model = query.ToList();

            if (statuses != null && statuses.Any())
            {
                model = model.Where(v => statuses.Contains(v.State.ToString())).ToList();
            }

            return PartialView("~/Views/AdminDashboard/Pages/Vehicle/_VehicleTablePartial.cshtml", model);
        }
    }
}