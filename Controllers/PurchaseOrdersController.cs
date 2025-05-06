// In PurchaseOrdersController.cs
using Microsoft.AspNetCore.Mvc;
using ECommerceProcurementSystem.Services;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerceProcurementSystem.Controllers
{
    public class PurchaseOrdersController : Controller
    {
        private readonly ISocrataService _svc;
        public PurchaseOrdersController(ISocrataService svc) => _svc = svc;

        // GET /PurchaseOrders?page=1
        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 100; // 100 orders per page
            var orders = await _svc.GetPurchaseOrdersAsync(pageSize, (page - 1) * pageSize);

            ViewBag.Page = page;
            // TODO: Consider adding total count / pagination controls to the view
            return View(orders);
        }

        // GET /PurchaseOrders/Details/PO-2024-005432
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Purchase Order ID cannot be empty.");
            }

            // Call the new service method to fetch only the specific PO
            var order = await _svc.GetPurchaseOrderByIdAsync(id);

            if (order is null)
            {
                // The specific PO was not found via the API
                return NotFound($"Purchase Order with ID '{id}' not found.");
            }

            return View(order);
        }
    }
}