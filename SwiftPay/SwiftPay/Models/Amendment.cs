using System;
using SwiftPay.Constants.Enums;

namespace SwiftPay.Domain.Remittance.Entities
{
    public class Amendment
    {
        public int AmendmentID { get; set; }
        public int RemitID { get; set; }
        public string FieldChanged { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public int RequestedBy { get; set; }
        public DateTime RequestedDate { get; set; }
        public AmendmentStatus Status { get; set; }
        
        // Audit Fields
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation (Optional for Phase-1)
        //public RemittanceRequest RemittanceRequest { get; set; }

        //File changed
    }
}
