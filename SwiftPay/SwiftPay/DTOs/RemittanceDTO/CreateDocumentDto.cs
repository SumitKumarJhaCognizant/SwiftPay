using System.ComponentModel.DataAnnotations;
using SwiftPay.Constants.Enums;

namespace SwiftPay.DTOs.RemittanceDTO
{
    public class CreateDocumentDto
    {
        [Required]
        public int RemitId { get; set; }

        [Required]
        public DocumentType DocType { get; set; }

        [StringLength(2048)]
        public string? FileURI { get; set; }
    }
}
