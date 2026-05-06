using System;
using System.ComponentModel.DataAnnotations;
using SwiftPay.Constants.Enums;

namespace SwiftPay.DTOs.KycAuditRecordDTO
{
    public class CreateKYCDocumentDto
    {
        [Required]
        public int KYCID { get; set; }

        [Required]
        public KYCDocumentType DocType { get; set; }

        [Required]
        [StringLength(2048)]
        public string FileURI { get; set; } = string.Empty;

        public string? Notes { get; set; }
    }

    public class UpdateKYCDocumentStatusDto
    {
        [Required]
        public KycVerificationStatus VerificationStatus { get; set; }

        public string? Notes { get; set; }
    }

    public class KYCDocumentResponseDto
    {
        public int KYCDocumentId { get; set; }
        public int KYCID { get; set; }
        public KYCDocumentType DocType { get; set; }
        public string FileURI { get; set; } = string.Empty;
        public DateTime UploadedDate { get; set; }
        public KycVerificationStatus VerificationStatus { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
