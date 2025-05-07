using ECommerceProcurementSystem.Models; // Assuming Commodity model is here

namespace ECommerceProcurementSystem.Models
{
    public class PurchaseOrderLine
    {
        // Foreign keys (Already initialized in your previous code)
        public string Purchase_Order { get; set; } = string.Empty;
        public string CommodityID { get; set; } = string.Empty;

        // Navigation property - Make nullable to satisfy CS8618
        public Commodity? Commodity { get; set; }

        // Line‑item fields (Strings already initialized, Nullable decimals are fine)
        public string Extended_Description { get; set; } = string.Empty;
        public decimal? Quantity_Ordered { get; set; }
        public string Unit_Of_Measure_Code { get; set; } = string.Empty;
        public string Unit_Of_Measure_Description { get; set; } = string.Empty;
        public decimal? Unit_Price { get; set; }
        public decimal? Line_Item_Total_Amount { get; set; }
    }
}