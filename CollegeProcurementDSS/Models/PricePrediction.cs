using System;
using System.ComponentModel.DataAnnotations;

namespace CollegeProcurementDSS.Models
{
    public class PricePrediction
    {
        [Key]
        public int Id { get; set; } // Главный ключ для БД

        public DateTime Date { get; set; }
        public string Item_ID { get; set; }
        public string Item_Name { get; set; }

        public decimal Real_Price_KZT { get; set; }
        public decimal AI_Predicted_Price_KZT { get; set; }

        // Автоматически считаем процент инфляции прямо в коде
        public decimal PriceDiffPercent =>
            Real_Price_KZT == 0 ? 0 : ((AI_Predicted_Price_KZT - Real_Price_KZT) / Real_Price_KZT) * 100;
    }
}