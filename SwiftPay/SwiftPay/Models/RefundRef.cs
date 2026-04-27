using System;
using System.ComponentModel.DataAnnotations.Schema;
using SwiftPay.Constants.Enums;

namespace SwiftPay.Domain.Remittance.Entities
{
    public class RefundRef
    {
        public int RefundID { get; set; }
        public int RemitID { get; set; }
        public decimal Amount { get; set; }
        public DateTime? RefundDate { get; set; }
        public RefundMethod Method { get; set; }
        public RefundStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsDeleted { get; set; }

        [ForeignKey(nameof(RemitID))]
        public RemittanceRequest? RemittanceRequest { get; set; }
    }
}