using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ECommerceProcurementSystem.Models; // Assuming ErrorViewModel is here
using ECommerceProcurementSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerceProcurementSystem.Controllers // Ensure namespace matches your project
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ProcurementContext _context;

        public HomeController(ILogger<HomeController> logger, ProcurementContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // --- Added this Action Method ---
        public IActionResult DataExploration()
        {
            // Add logic here later if needed (e.g., get data for charts)
            return View();
        }
        // --- End of Added Method ---

        /// <summary>
        /// API endpoint for Data Exploration: returns total sales per year for Chart.js
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAnnualSalesData()
        {
            var data = await _context.AnnualReports
                .GroupBy(r => r.Year)
                .Select(g => new { Year = g.Key, TotalSales = g.Sum(x => x.SaleAmount) })
                .OrderBy(x => x.Year)
                .ToListAsync();
            return Json(data);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}