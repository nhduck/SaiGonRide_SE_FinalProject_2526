using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalVehicleService.Data;
using RentalVehicleService.Models;

namespace RentalVehicleService.Controllers
{
    public class BugReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string ViewPath = "~/Views/AdminDashboard/Pages/BugReports/";

        public BugReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: BugReports — trả PartialView vào AdminDashboard
        public async Task<IActionResult> Index()
        {
            var reports = await _context.BugReport.OrderByDescending(r => r.CreatedDate).ToListAsync();

            ViewBag.TotalReports = reports.Count;
            ViewBag.NewReports   = reports.Count(r => r.Status == BugReport.BugStatus.New);
            ViewBag.InProgress   = reports.Count(r => r.Status == BugReport.BugStatus.InProgress);
            ViewBag.Resolved     = reports.Count(r => r.Status == BugReport.BugStatus.Resolved);
            ViewBag.Closed       = reports.Count(r => r.Status == BugReport.BugStatus.Closed);

            return PartialView($"{ViewPath}Index.cshtml", reports);
        }

        // GET: BugReports/Details/5 — trả PartialView cho modal
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var report = await _context.BugReport.FirstOrDefaultAsync(r => r.Id == id);
            if (report == null) return NotFound();
            return PartialView($"{ViewPath}_DetailsBody.cshtml", report);
        }

        // GET: BugReports/Edit/5 — trả JSON cho JS
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var report = await _context.BugReport.FindAsync(id);
            if (report == null) return NotFound();

            return Ok(new
            {
                id          = report.Id,
                title       = report.Title,
                description = report.Description,
                createdDate = report.CreatedDate.ToString("yyyy-MM-ddTHH:mm"),
                status      = (int)report.Status
            });
        }

        // POST: BugReports/Edit — chỉ cập nhật Status
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int Status)
        {
            var report = await _context.BugReport.FindAsync(id);
            if (report == null) return NotFound();

            if (!Enum.IsDefined(typeof(BugReport.BugStatus), Status))
                return BadRequest(new { success = false, errors = new[] { "Invalid status value." } });

            report.Status = (BugReport.BugStatus)Status;

            try
            {
                _context.Update(report);
                await _context.SaveChangesAsync();
                return Ok(new { success = true });
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest(new { success = false, errors = new[] { "Concurrency error. Please try again." } });
            }
        }

        // POST: BugReports/DeleteConfirmed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var report = await _context.BugReport.FindAsync(id);
            if (report != null)
            {
                _context.BugReport.Remove(report);
                await _context.SaveChangesAsync();
            }
            return Ok(new { success = true });
        }

        // GET: BugReports/GetAmountInfo
        [HttpGet]
        public async Task<IActionResult> GetAmountInfo()
        {
            var reports = await _context.BugReport.ToListAsync();
            return Ok(new
            {
                totalReports = reports.Count,
                newReports   = reports.Count(r => r.Status == BugReport.BugStatus.New),
                inProgress   = reports.Count(r => r.Status == BugReport.BugStatus.InProgress),
                resolved     = reports.Count(r => r.Status == BugReport.BugStatus.Resolved),
                closed       = reports.Count(r => r.Status == BugReport.BugStatus.Closed)
            });
        }

        // GET: BugReports/Search
        [HttpGet]
        public async Task<IActionResult> Search(string searchTerm, List<string> statuses)
        {
            var query = _context.BugReport.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(r =>
                    r.Title.ToLower().Contains(term) ||
                    r.Description.ToLower().Contains(term) ||
                    r.Id.ToString().Contains(term));
            }

            var reports = await query.OrderByDescending(r => r.CreatedDate).ToListAsync();

            if (statuses != null && statuses.Any())
                reports = reports.Where(r => statuses.Contains(r.Status.ToString())).ToList();

            return PartialView($"{ViewPath}_ReportTablePartial.cshtml", reports);
        }

        private bool BugReportExists(int id) =>
            _context.BugReport.Any(e => e.Id == id);
    }
}
