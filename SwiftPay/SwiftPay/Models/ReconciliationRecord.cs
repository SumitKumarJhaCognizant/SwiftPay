using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SwiftPay.Constants.Enums;
namespace SwiftPay.Models
{
    public class ReconciliationRecord
    {
        public int ReconID { get; set; }

        public ReferenceType ReferenceType { get; set; }  // Remit, Instruction, PartnerAck

		public string ReferenceID { get; set; }

        public decimal ExpectedAmount { get; set; }

        public decimal ActualAmount { get; set; }

        public Result Result { get; set; } // Matched, Mismatched

        public DateTime ReconDate { get; set; }

        public string? Notes { get; set; } // Optional

        public DateTime CreatedDate { get; set; }
		public DateTime UpdateDate { get; set; }

		public bool IsDeleted { get; set; }

    }
}
