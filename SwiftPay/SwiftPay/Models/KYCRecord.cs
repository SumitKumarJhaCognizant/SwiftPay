using System;
using Model;
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

        // navigation
        public User User { get; set; }
    }
}