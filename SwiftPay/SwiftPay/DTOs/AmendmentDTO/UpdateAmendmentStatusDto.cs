using System.ComponentModel.DataAnnotations;

namespace SwiftPay.DTOs.AmendmentDTO
{
    public class UpdateAmendmentStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}
