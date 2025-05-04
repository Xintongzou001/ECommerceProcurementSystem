using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ECommerceProcurementSystem.Data;
using ECommerceProcurementSystem.Models;

namespace ECommerceProcurementSystem.Controllers
{
    public class AnnualSaleAmountController : Controller
    {
        private readonly ProcurementContext _context;

        public AnnualSaleAmountController(ProcurementContext context)
        {
            _context = context;
        }

        // GET: AnnualSaleAmount
        [HttpGet] // <<< --- ADDED THIS ATTRIBUTE --- >>>
        public async Task<IActionResult> Index()
        {
            // Added null check for safety, though _context.AnnualSaleAmounts might handle it
            if (_context.AnnualSaleAmounts == null)
            {
                return Problem("Entity set 'ProcurementContext.AnnualSaleAmounts' is null.");
            }
            var procurementContext = _context.AnnualSaleAmounts.Include(a => a.City).Include(a => a.Vendor);
            return View(await procurementContext.ToListAsync());
        }

        // GET: AnnualSaleAmount/Details/5
        [HttpGet] // Adding HttpGet for clarity/consistency
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.AnnualSaleAmounts == null) // Added null check
            {
                return NotFound();
            }

            var annualSaleAmount = await _context.AnnualSaleAmounts
                .Include(a => a.City)
                .Include(a => a.Vendor)
                .FirstOrDefaultAsync(m => m.ID == id); // Assuming primary key is named ID in the model
            if (annualSaleAmount == null)
            {
                return NotFound();
            }

            return View(annualSaleAmount);
        }

        // GET: AnnualSaleAmount/Create
        [HttpGet] // This was added previously
        public IActionResult Create()
        {
            // Consider displaying CityName and VendorName in the dropdown for better usability
            ViewData["CityID"] = new SelectList(_context.Cities, "CityID", "CityName"); // Changed display field
            ViewData["Vendor_Code"] = new SelectList(_context.Vendors, "Vendor_Code", "VendorName"); // Changed display field (assuming VendorName exists)
            return View();
        }

        // POST: AnnualSaleAmount/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,CityID,Vendor_Code,Year,SaleAmount")] AnnualSaleAmount annualSaleAmount)
        {
            if (ModelState.IsValid)
            {
                _context.Add(annualSaleAmount);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CityID"] = new SelectList(_context.Cities, "CityID", "CityName", annualSaleAmount.CityID); // Changed display field
            ViewData["Vendor_Code"] = new SelectList(_context.Vendors, "Vendor_Code", "VendorName", annualSaleAmount.Vendor_Code); // Changed display field
            return View(annualSaleAmount);
        }

        // GET: AnnualSaleAmount/Edit/5
        [HttpGet] // This was added previously for clarity
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.AnnualSaleAmounts == null) // Added null check
            {
                return NotFound();
            }

            var annualSaleAmount = await _context.AnnualSaleAmounts.FindAsync(id); // FindAsync uses primary key
            if (annualSaleAmount == null)
            {
                return NotFound();
            }
            ViewData["CityID"] = new SelectList(_context.Cities, "CityID", "CityName", annualSaleAmount.CityID); // Changed display field
            ViewData["Vendor_Code"] = new SelectList(_context.Vendors, "Vendor_Code", "VendorName", annualSaleAmount.Vendor_Code); // Changed display field
            return View(annualSaleAmount);
        }

        // POST: AnnualSaleAmount/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,CityID,Vendor_Code,Year,SaleAmount")] AnnualSaleAmount annualSaleAmount)
        {
            if (id != annualSaleAmount.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(annualSaleAmount);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnnualSaleAmountExists(annualSaleAmount.ID))
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
            ViewData["CityID"] = new SelectList(_context.Cities, "CityID", "CityName", annualSaleAmount.CityID); // Changed display field
            ViewData["Vendor_Code"] = new SelectList(_context.Vendors, "Vendor_Code", "VendorName", annualSaleAmount.Vendor_Code); // Changed display field
            return View(annualSaleAmount);
        }

        // GET: AnnualSaleAmount/Delete/5
        [HttpGet] // This was added previously for clarity
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.AnnualSaleAmounts == null) // Added null check
            {
                return NotFound();
            }

            var annualSaleAmount = await _context.AnnualSaleAmounts
                .Include(a => a.City)
                .Include(a => a.Vendor)
                .FirstOrDefaultAsync(m => m.ID == id); // Assuming primary key is named ID
            if (annualSaleAmount == null)
            {
                return NotFound();
            }

            return View(annualSaleAmount);
        }

        // POST: AnnualSaleAmount/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.AnnualSaleAmounts == null) // Added null check
            {
                return Problem("Entity set 'ProcurementContext.AnnualSaleAmounts' is null.");
            }
            var annualSaleAmount = await _context.AnnualSaleAmounts.FindAsync(id);
            if (annualSaleAmount != null)
            {
                _context.AnnualSaleAmounts.Remove(annualSaleAmount);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AnnualSaleAmountExists(int id)
        {
            return (_context.AnnualSaleAmounts?.Any(e => e.ID == id)).GetValueOrDefault();
        }
    }
}