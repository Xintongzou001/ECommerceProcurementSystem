using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceProcurementSystem.Models
{
    public class Vendor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]  // because API provides the key
        // Initialize string properties to satisfy CS8618
        public string Vendor_Code { get; set; } = string.Empty;

        [Display(Name = "Vendor Name")]
        public string VendorName { get; set; } = string.Empty; // previously called "Vendor" in API

        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Zip { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;

        // Navigation: one vendor → many annual reports
        // Initialize collection properties to satisfy CS8618
        // Add 'virtual' for potential EF Core lazy loading benefits
        public virtual ICollection<AnnualReport> AnnualReports { get; set; } = new List<AnnualReport>();

        // Optional: one vendor → many purchase orders
        public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
    }
}