// In PurchaseOrder.cs
using System.Collections.Generic;
using ECommerceProcurementSystem.Models; // Assuming models are in this namespace

namespace ECommerceProcurementSystem.Models
{
    public class PurchaseOrder
    {
        // Initialize Primary Key to satisfy CS8618
        public string Purchase_Order { get; set; } = string.Empty;

        // --- ADD FOREIGN KEY PROPERTIES START ---
        // Add the properties used in HasForeignKey in ProcurementContext.cs
        // Make them nullable (?) if a PurchaseOrder might exist without a Vendor/Agreement
        public string? Vendor_Code { get; set; }
        public string? Master_Agreement { get; set; }
        // --- ADD FOREIGN KEY PROPERTIES END ---

        // Navigation properties - make nullable (?) to satisfy CS8618
        public Vendor? Vendor { get; set; }
        public MasterAgreement? Agreement { get; set; }

        // Collection of line items - Initialize to satisfy CS8618
        public List<PurchaseOrderLine> Lines { get; set; } = new();
    }
}