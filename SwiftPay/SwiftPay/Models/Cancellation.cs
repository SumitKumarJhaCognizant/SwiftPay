using System;
using System.ComponentModel.DataAnnotations.Schema;
using SwiftPay.Constants.Enums;

namespace SwiftPay.Domain.Remittance.Entities
{
    public class Cancellation
    {
        public int CancellationID { get; set; }
        public int RemitID { get; set; }
        public string Reason { get; set; }
        public DateTime RequestedDate { get; set; }
        public CancellationStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsDeleted { get; set; }

        [ForeignKey(nameof(RemitID))]
        public RemittanceRequest? RemittanceRequest { get; set; }
    }
}