using System.ComponentModel.DataAnnotations;

namespace SwiftPay.DTOs.RemitReportDTO
{
    public class CreateRemitReportDto
    {
        [Required]
        [StringLength(200)]
        public string Scope { get; set; }

        [Required]
        public string Metrics { get; set; } // JSON string

        // Optional - allow service/database to set if not provided
        public System.DateTime? GeneratedDate { get; set; }
    }
}
