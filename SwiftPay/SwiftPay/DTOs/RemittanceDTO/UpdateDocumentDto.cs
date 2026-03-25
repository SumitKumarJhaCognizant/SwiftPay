using System.ComponentModel.DataAnnotations;

namespace SwiftPay.DTOs.RemittanceDTO
{
    public class UpdateDocumentDto
    {
        [Required]
        public int DocumentId { get; set; }

        [Required]
        [StringLength(2048)]
        public string FileURI { get; set; } 

        public string? VerificationStatus { get; set; }

        public string DocType { get; set; } // Added property

        public bool IsDeleted { get; set; } // Added property
    }
}
