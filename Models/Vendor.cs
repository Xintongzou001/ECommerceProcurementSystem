using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceProcurementSystem.Models
{
    public class Vendor
    {
        [Key]
        public int Vendor_Code { get; set; }
        public string VendorName { get; set; }

        public ICollection<AnnualSaleAmount> AnnualSaleAmounts { get; set; }
    }
}