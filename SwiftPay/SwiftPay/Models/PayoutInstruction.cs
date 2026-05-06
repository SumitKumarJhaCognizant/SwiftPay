using System;
using SwiftPay.Constants.Enums;

namespace SwiftPay.Domain.Remittance.Entities
{
    public class PayoutInstruction
    {
        public string InstructionId { get; set; }
        public string RemitId { get; set; }
        public string PartnerCode { get; set; }
        public string PayloadJson { get; set; }
        public string? AckRef { get; set; }
         public PayOutInstructionStatus PartnerStatus { get; set; }// Sent, Ack, Rejected, Settled
         public DateTimeOffset SentDate { get; set; }
         public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset UpdateDate { get; set; }
        public bool IsDeleted { get; set; }

         
        public byte[] RowVersion { get; set; }
    }
}