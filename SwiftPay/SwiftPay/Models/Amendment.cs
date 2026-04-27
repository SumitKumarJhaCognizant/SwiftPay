using System;
using System.ComponentModel.DataAnnotations.Schema;
using SwiftPay.Constants.Enums;
using SwiftPay.Models;

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
        [ForeignKey(nameof(RemitID))]
        public RemittanceRequest? RemittanceRequest { get; set; }

        // Link the requester to the application User who created/requested the amendment.
        // RequestedBy already holds the foreign key integer (User.UserId).
        [ForeignKey(nameof(RequestedBy))]
        public User? RequestedByUser { get; set; }

        
    }
}
