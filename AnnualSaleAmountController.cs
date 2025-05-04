using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ECommerceProcurementSystem.Data;
using ECommerceProcurementSystem.Models;

namespace ECommerceProcurementSystem
{
    public class AnnualSaleAmountController : Controller
    {
        private readonly ProcurementContext _context;

        public AnnualSaleAmountController(ProcurementContext context)
        {
            _context = context;
        }

        // GET: AnnualSaleAmounts
        public async Task<IActionResult> Index()
        {
            var procurementContext = _context.AnnualSaleAmounts.Include(a => a.City).Include(a => a.Vendor);
            return View(await procurementContext.ToListAsync());
        }

        // GET: AnnualSaleAmounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var annualSaleAmount = await _context.AnnualSaleAmounts
                .Include(a => a.City)
                .Include(a => a.Vendor)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (annualSaleAmount == null)
            {
                return NotFound();
            }

            return View(annualSaleAmount);
        }

        // GET: AnnualSaleAmounts/Create
        public IActionResult Create()
        {
            ViewData["CityID"] = new SelectList(_context.Cities, "CityID", "CityID");
            ViewData["Vendor_Code"] = new SelectList(_context.Vendors, "Vendor_Code", "Vendor_Code");
            return View();
        }

        // POST: AnnualSaleAmounts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
            ViewData["CityID"] = new SelectList(_context.Cities, "CityID", "CityID", annualSaleAmount.CityID);
            ViewData["Vendor_Code"] = new SelectList(_context.Vendors, "Vendor_Code", "Vendor_Code", annualSaleAmount.Vendor_Code);
            return View(annualSaleAmount);
        }

        // GET: AnnualSaleAmounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var annualSaleAmount = await _context.AnnualSaleAmounts.FindAsync(id);
            if (annualSaleAmount == null)
            {
                return NotFound();
            }
            ViewData["CityID"] = new SelectList(_context.Cities, "CityID", "CityID", annualSaleAmount.CityID);
            ViewData["Vendor_Code"] = new SelectList(_context.Vendors, "Vendor_Code", "Vendor_Code", annualSaleAmount.Vendor_Code);
            return View(annualSaleAmount);
        }

        // POST: AnnualSaleAmounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
            ViewData["CityID"] = new SelectList(_context.Cities, "CityID", "CityID", annualSaleAmount.CityID);
            ViewData["Vendor_Code"] = new SelectList(_context.Vendors, "Vendor_Code", "Vendor_Code", annualSaleAmount.Vendor_Code);
            return View(annualSaleAmount);
        }

        // GET: AnnualSaleAmounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var annualSaleAmount = await _context.AnnualSaleAmounts
                .Include(a => a.City)
                .Include(a => a.Vendor)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (annualSaleAmount == null)
            {
                return NotFound();
            }

            return View(annualSaleAmount);
        }

        // POST: AnnualSaleAmounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
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
            return _context.AnnualSaleAmounts.Any(e => e.ID == id);
        }
    }
}
