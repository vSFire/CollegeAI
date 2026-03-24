using System;
using System.ComponentModel.DataAnnotations;

namespace CollegeProcurementDSS.Models
{
    public class ProcurementHistory
    {
        [Key]
        public int Id { get; set; }

        public DateTime Date { get; set; }
        public string Item_ID { get; set; }
        public string Item_Name { get; set; }
        public string Target_Specialty { get; set; }

        public decimal Real_Price_KZT { get; set; }
        public decimal Current_Stock { get; set; }
        public int Daily_Consumption { get; set; }
    }
}