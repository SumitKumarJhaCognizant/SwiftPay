using System.ComponentModel.DataAnnotations;

namespace SwiftPay.DTOs.RemittanceDTO
{
    public class CreateDocumentDto
    {
        [Required]
        public int RemitId { get; set; }

        [Required]
        public int DocType { get; set; }

        [Required]
        [StringLength(2048)]
        public string FileURI { get; set; } 
    }
}
