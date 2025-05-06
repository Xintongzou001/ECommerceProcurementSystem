// In ISocrataService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using ECommerceProcurementSystem.Models;

namespace ECommerceProcurementSystem.Services
{
    public interface ISocrataService
    {
        /// <summary>
        /// Returns purchase orders and their lines, already grouped into rich objects.
        /// LIMIT is rows *from the raw dataset*; 50000 max per call with AppToken.
        /// </summary>
        Task<IReadOnlyList<PurchaseOrder>> GetPurchaseOrdersAsync(int limit = 1000, int offset = 0);

        /// <summary>
        /// Returns a single purchase order and its lines by Purchase Order ID.
        /// Returns null if the purchase order is not found.
        /// </summary>
        Task<PurchaseOrder?> GetPurchaseOrderByIdAsync(string id); // <<< --- ADD THIS LINE --- >>>
    }
}