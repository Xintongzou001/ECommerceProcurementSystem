using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ECommerceProcurementSystem.Models; // Assuming ErrorViewModel is here

namespace ECommerceProcurementSystem.Controllers // Ensure namespace matches your project
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
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


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}