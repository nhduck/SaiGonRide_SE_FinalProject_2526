using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RentalVehicleService.Controllers
{
    public class AdminDashboardController : Controller
    {
        public IActionResult Index() => View();

        public IActionResult Dashboard()
            => PartialView("~/Views/AdminDashboard/Pages/Dashboard/Index.cshtml");

        public IActionResult VehicleManagement()
            => PartialView("~/Views/AdminDashboard/Pages/VehicleManagement/Index.cshtml");

        public IActionResult StationManagement()
            => PartialView("~/Views/AdminDashboard/Pages/StationManagement/Index.cshtml");

        public IActionResult UserManagement()
            => PartialView("~/Views/AdminDashboard/Pages/UserManagement/Index.cshtml");

        public IActionResult Reports()
            => PartialView("~/Views/AdminDashboard/Pages/Reports/Index.cshtml");
    }
}
