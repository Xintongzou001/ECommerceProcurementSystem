using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// using System.Text.Json.Serialization; // Removed as likely unused in this model

namespace ECommerceProcurementSystem.Models
{
    public class AnnualReport
    {
        [Key]
        public int ID { get; set; }

        // Foreign key properties (value types, generally okay with CS8618 unless complex constructor logic exists)
        public int CityID { get; set; }
        public int Vendor_Code { get; set; } // Keep as int for now, but see note below

        public int Year { get; set; }
        public decimal SaleAmount { get; set; }

        // Navigation properties made nullable (?) to resolve CS8618 warnings
        public City? City { get; set; }
        public Vendor? Vendor { get; set; }
    }
}