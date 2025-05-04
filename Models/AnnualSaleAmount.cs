using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ECommerceProcurementSystem.Models
{
    public class AnnualSaleAmount
    {
        [Key]
        public int ID { get; set; }
        public int CityID { get; set; }
        public int Vendor_Code { get; set; }
        public int Year { get; set; }
        public decimal SaleAmount { get; set; }

        public City City { get; set; }
        public Vendor Vendor { get; set; }
    }
}