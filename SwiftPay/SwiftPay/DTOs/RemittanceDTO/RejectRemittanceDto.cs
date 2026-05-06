using System.ComponentModel.DataAnnotations;

namespace SwiftPay.DTOs.RemittanceDTO
{
    public class RejectRemittanceDto
    {
        [Required]
        [StringLength(500, MinimumLength = 3, ErrorMessage = "Reason must be between 3 and 500 characters.")]
        public string Reason { get; set; } = string.Empty;
    }
}
