using System;

namespace SwiftPay.DTOs.RemittanceDTO
{
    public class DocumentResponseDto
    {
        public int DocumentId { get; set; }
        public int RemitId { get; set; }
        public string DocType { get; set; } = string.Empty;
        public string FileURI { get; set; } = string.Empty;
        public string VerificationStatus { get; set; } = string.Empty;
        public DateTimeOffset UploadedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; } // Added property
    }
}
