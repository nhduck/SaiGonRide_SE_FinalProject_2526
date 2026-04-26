using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RentalVehicleService.Data;
using RentalVehicleService.Models;

namespace RentalVehicleService.Controllers
{
    public class BugReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BugReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: BugReports
        public async Task<IActionResult> Index()
        {
            return View(await _context.BugReport.ToListAsync());
        }

        // GET: BugReports/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bugReport = await _context.BugReport
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bugReport == null)
            {
                return NotFound();
            }

            return View(bugReport);
        }

        // GET: BugReports/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BugReports/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,CreatedDate,Status")] BugReport bugReport)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bugReport);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(bugReport);
        }

        // GET: BugReports/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bugReport = await _context.BugReport.FindAsync(id);
            if (bugReport == null)
            {
                return NotFound();
            }
            return View(bugReport);
        }

        // POST: BugReports/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,CreatedDate,Status")] BugReport bugReport)
        {
            if (id != bugReport.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bugReport);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BugReportExists(bugReport.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(bugReport);
        }

        // GET: BugReports/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bugReport = await _context.BugReport
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bugReport == null)
            {
                return NotFound();
            }

            return View(bugReport);
        }

        // POST: BugReports/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bugReport = await _context.BugReport.FindAsync(id);
            if (bugReport != null)
            {
                _context.BugReport.Remove(bugReport);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BugReportExists(int id)
        {
            return _context.BugReport.Any(e => e.Id == id);
        }
    }
}
