using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ECommerceProcurementSystem.Models;


namespace ECommerceProcurementSystem.Controllers
{
    public class AboutUsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
