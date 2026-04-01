using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RentalVehicleService.Controllers
{
    public class AdminDashboardController : Controller
    {
        // GET: AdminDashboardController
        public ActionResult Index()
        {
            return View();
        }

        // GET: AdminDashboardController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: AdminDashboardController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AdminDashboardController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: AdminDashboardController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: AdminDashboardController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: AdminDashboardController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: AdminDashboardController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
