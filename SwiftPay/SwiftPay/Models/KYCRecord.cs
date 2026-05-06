using System;
using SwiftPay.Models;
using SwiftPay.Constants.Enums;

namespace SwiftPay.Domain.Remittance.Entities
{
    public class KYCRecord
    {
        public int KYCID { get; set; }

        // Foreign key to User
        public int UserID { get; set; }

        public KYCLevel KYCLevel { get; set; }

        public KycVerificationStatus VerificationStatus { get; set; }

        public DateTime? VerifiedDate { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }

        // navigation (optional to play nicely with User's soft-delete query filter)
        public User? User { get; set; }
    }
}