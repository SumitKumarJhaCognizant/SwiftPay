using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SwiftPay.Models
{
	public class SettlementBatch
	{
		[Key]
		public int BatchID { get; set; }
		
		public string Corridor { get; set; }
		public DateTime PeriodStart { get; set; }
		public DateTime PeriodEnd { get; set; }
		public int ItemCount { get; set; }
		public decimal TotalSendAmount { get; set; }
		public decimal TotalPayoutAmount { get; set; }
		public DateTime CreatedDate { get; set; }
		public string Status { get; set; }



	}
}
