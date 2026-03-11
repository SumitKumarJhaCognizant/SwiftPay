using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SwiftPay.Constants.Enums;
namespace SwiftPay.Models
{
    
    public class SettlementBatch
    {
        [Key]
        public int BatchID { get; set; }

       
        public string Corridor { get; set; }

  
        public DateTime PeriodStart { get; set; }

   
        public DateTime PeriodEnd { get; set; }

        public int ItemCount { get; set; } = 0;

       
        public decimal TotalSendAmount { get; set; } = 0.0000m;

      
        public decimal TotalPayoutAmount { get; set; } = 0.0000m;

        public DateTime CreatedDate { get; set; } 

    
        public Status Status { get; set; } 
    }
}
