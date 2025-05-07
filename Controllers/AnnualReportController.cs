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
using ECommerceProcurementSystem.Services;

// Ensure namespace is correct
namespace ECommerceProcurementSystem.Controllers
{
    // Controller name matches the entity we are managing
    public class AnnualReportController : Controller
    {
        private readonly ProcurementContext _context;
        private readonly ISocrataService _socrataService;

        /// <summary>
        /// Controller for managing Annual Reports, including offline CRUD and initial import from Socrata API.
        /// </summary>
        public AnnualReportController(ProcurementContext context, ISocrataService socrataService)
        {
            _context = context;
            _socrataService = socrataService;
        }

        // GET: AnnualReport (Index View)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (_context.AnnualReports == null)
                return Problem("Entity set 'ProcurementContext.AnnualReports' is null.");

            // If no data, import from Socrata API (limit 50)
            if (!await _context.AnnualReports.AnyAsync())
            {
                try
                {
                    // Step 1: Get the data from Socrata API
                    var importedReports = await _socrataService.GetAnnualReportsFromSocrataAsync(50);
                    
                    // Step 2: Process vendors first (they're referenced by AnnualReports)
                    var vendorCodes = importedReports
                        .Select(r => r.Vendor_Code)
                        .Distinct()
                        .ToList();
                    
                    foreach (var vendorCode in vendorCodes)
                    {
                        if (!await _context.Vendors.AnyAsync(v => v.Vendor_Code == vendorCode))
                        {
                            // Create a basic vendor entry if it doesn't exist
                            _context.Vendors.Add(new Vendor
                            {
                                Vendor_Code = vendorCode,
                                VendorName = vendorCode, // Use code as name temporarily
                                Address = string.Empty,
                                City = string.Empty,
                                Zip = string.Empty,
                                Country = string.Empty
                            });
                        }
                    }
                    
                    // Save vendors to database so they have valid IDs
                    await _context.SaveChangesAsync();
                    
                    // Step 3: Process cities (also referenced by AnnualReports)
                    var cityDict = new Dictionary<string, int>();
                    
                    foreach (var report in importedReports)
                    {
                        string? cityName = report.City?.CityName;
                        
                        if (!string.IsNullOrEmpty(cityName) && !cityDict.ContainsKey(cityName))
                        {
                            // Check if city already exists
                            var existingCity = await _context.Cities
                                .FirstOrDefaultAsync(c => c.CityName.ToLower() == cityName.ToLower());
                            
                            if (existingCity != null)
                            {
                                cityDict[cityName] = existingCity.CityID;
                            }
                            else
                            {
                                // Create a new city
                                var newCity = new City { CityName = cityName };
                                _context.Cities.Add(newCity);
                                
                                // Save immediately to get the generated ID
                                await _context.SaveChangesAsync();
                                
                                cityDict[cityName] = newCity.CityID;
                            }
                        }
                    }
                    
                    // Step 4: Create AnnualReport entries with correct foreign keys
                    foreach (var report in importedReports)
                    {
                        string? cityName = report.City?.CityName;
                        
                        if (!string.IsNullOrEmpty(cityName) && cityDict.ContainsKey(cityName))
                        {
                            var newReport = new AnnualReport
                            {
                                CityID = cityDict[cityName],
                                Vendor_Code = report.Vendor_Code,
                                Year = report.Year,
                                SaleAmount = report.SaleAmount
                                // Don't set navigation properties here to avoid tracking issues
                            };
                            
                            _context.AnnualReports.Add(newReport);
                        }
                    }
                    
                    // Save all annual reports
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // Log the exception - In a real app, use a proper logging framework
                    System.Diagnostics.Debug.WriteLine($"Error importing data: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    }
                    
                    // Return problem details
                    return Problem($"Error importing data: {ex.Message}");
                }
            }
            
            // Load reports with related entities for display
            var annualReports = _context.AnnualReports
                .Include(a => a.City)
                .Include(a => a.Vendor);
                
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