using System;
using System.ComponentModel.DataAnnotations; // <<< Add this using directive

namespace ECommerceProcurementSystem.Models
{
    public class MasterAgreement
    {
        [Key] // <<< Add this attribute to designate the primary key
        public string Master_Agreement { get; set; } = string.Empty; // Initialize

        public string Contract_Name { get; set; } = string.Empty; // Initialize
        public DateTime? Award_Date { get; set; } // Nullable DateTime is fine
    }
}