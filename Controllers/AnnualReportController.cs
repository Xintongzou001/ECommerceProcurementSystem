// In AnnualReportController.cs (Updated)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ECommerceProcurementSystem.Data;
using ECommerceProcurementSystem.Models;

// Ensure namespace is correct
namespace ECommerceProcurementSystem.Controllers
{
    // Controller name matches the entity we are managing
    public class AnnualReportController : Controller
    {
        private readonly ProcurementContext _context;

        public AnnualReportController(ProcurementContext context)
        {
            _context = context;
        }

        // GET: AnnualReport (Index View)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Use the correct DbSet: AnnualReports
            if (_context.AnnualReports == null)
            {
                return Problem("Entity set 'ProcurementContext.AnnualReports' is null.");
            }
            // Use correct DbSet and include navigation properties
            var annualReports = _context.AnnualReports.Include(a => a.City).Include(a => a.Vendor);
            return View(await annualReports.ToListAsync());
        }

        // GET: AnnualReport/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // Use the correct DbSet: AnnualReports
            if (_context.AnnualReports == null) return NotFound();


            var annualReport = await _context.AnnualReports
                .Include(a => a.City)
                .Include(a => a.Vendor)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (annualReport == null)
            {
                return NotFound();
            }

            // Pass the AnnualReport object to the View
            return View(annualReport);
        }

        // GET: AnnualReport/Create
        [HttpGet]
        public IActionResult Create()
        {
            // Populate ViewBag for dropdowns
            // Consider checking if _context.Cities or _context.Vendors are null
            ViewData["CityID"] = new SelectList(_context.Cities, "CityID", "CityName"); // Display CityName
            ViewData["Vendor_Code"] = new SelectList(_context.Vendors, "Vendor_Code", "VendorName"); // Display VendorName
            return View();
        }

        // POST: AnnualReport/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Bind to the AnnualReport model
        public async Task<IActionResult> Create([Bind("ID,CityID,Vendor_Code,Year,SaleAmount")] AnnualReport annualReport)
        {
            // Check related contexts if needed
            if (_context.AnnualReports == null || _context.Cities == null || _context.Vendors == null)
            {
                return Problem("Required context is null.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(annualReport);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // Repopulate ViewBag if validation fails
            ViewData["CityID"] = new SelectList(_context.Cities, "CityID", "CityName", annualReport.CityID);
            ViewData["Vendor_Code"] = new SelectList(_context.Vendors, "Vendor_Code", "VendorName", annualReport.Vendor_Code);
            return View(annualReport); // Return view with the submitted model
        }

        // GET: AnnualReport/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            // Use the correct DbSet: AnnualReports
            if (_context.AnnualReports == null || _context.Cities == null || _context.Vendors == null) return NotFound();


            var annualReport = await _context.AnnualReports.FindAsync(id);
            if (annualReport == null)
            {
                return NotFound();
            }
            // Populate ViewBag for dropdowns
            ViewData["CityID"] = new SelectList(_context.Cities, "CityID", "CityName", annualReport.CityID);
            ViewData["Vendor_Code"] = new SelectList(_context.Vendors, "Vendor_Code", "VendorName", annualReport.Vendor_Code);
            return View(annualReport); // Pass AnnualReport object to view
        }

        // POST: AnnualReport/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,CityID,Vendor_Code,Year,SaleAmount")] AnnualReport annualReport)
        {
            if (id != annualReport.ID)
            {
                return NotFound();
            }

            // Check related contexts if needed
            if (_context.AnnualReports == null || _context.Cities == null || _context.Vendors == null)
            {
                return Problem("Required context is null.");
            }


            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(annualReport);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnnualReportExists(annualReport.ID)) // Use updated helper method name
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
            // Repopulate ViewBag if validation fails
            ViewData["CityID"] = new SelectList(_context.Cities, "CityID", "CityName", annualReport.CityID);
            ViewData["Vendor_Code"] = new SelectList(_context.Vendors, "Vendor_Code", "VendorName", annualReport.Vendor_Code);
            return View(annualReport); // Return view with the submitted model
        }

        // GET: AnnualReport/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            // Use the correct DbSet: AnnualReports
            if (_context.AnnualReports == null) return NotFound();

            var annualReport = await _context.AnnualReports
                .Include(a => a.City)
                .Include(a => a.Vendor)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (annualReport == null)
            {
                return NotFound();
            }

            return View(annualReport); // Pass AnnualReport object to view
        }

        // POST: AnnualReport/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Use the correct DbSet: AnnualReports
            if (_context.AnnualReports == null)
            {
                return Problem("Entity set 'ProcurementContext.AnnualReports' is null.");
            }
            var annualReport = await _context.AnnualReports.FindAsync(id);
            if (annualReport != null)
            {
                _context.AnnualReports.Remove(annualReport);
                await _context.SaveChangesAsync(); // Save changes after removing
            }
            // Removed redundant SaveChangesAsync call here

            return RedirectToAction(nameof(Index));
        }

        // Renamed helper method and use correct DbSet
        private bool AnnualReportExists(int id)
        {
            // Use the correct DbSet: AnnualReports
            return (_context.AnnualReports?.Any(e => e.ID == id)).GetValueOrDefault();
        }
    }
}