using System.ComponentModel.DataAnnotations;

namespace SwiftPay.DTOs.CancellationDTO
{
    public class CreateCancellationDto
    {
        [Required]
        public int RemitId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Reason { get; set; }
    }
}
