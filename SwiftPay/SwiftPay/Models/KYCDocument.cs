using System;
using SwiftPay.Constants.Enums;
using SwiftPay.Domain.Remittance.Entities;

namespace SwiftPay.Models
{
    public class KYCDocument
    {
        public int KYCDocumentId { get; set; }

        // Foreign key to KYCRecord
        public int KYCID { get; set; }

        public KYCDocumentType DocType { get; set; }

        public string FileURI { get; set; } = string.Empty;

        public DateTime UploadedDate { get; set; }

        public KycVerificationStatus VerificationStatus { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }

        // Navigation
        public KYCRecord? KYC { get; set; }
    }
}
