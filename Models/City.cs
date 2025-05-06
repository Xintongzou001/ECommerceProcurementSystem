// In City.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceProcurementSystem.Models
{
    public class City
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CityID { get; set; }

        // Initialize CityName to ensure it's non-null
        public string CityName { get; set; } = string.Empty; // <<< MODIFIED LINE (Added = string.Empty;)

        // Collection uses AnnualReport now and is initialized
        public virtual ICollection<AnnualReport> AnnualReports { get; set; } = new List<AnnualReport>();
    }
}