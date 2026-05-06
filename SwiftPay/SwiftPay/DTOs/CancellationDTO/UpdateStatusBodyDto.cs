using System.ComponentModel.DataAnnotations;

namespace SwiftPay.DTOs.CancellationDTO
{
    /// <summary>
    /// Generic status-update wrapper. Used by Cancellation and RefundRef PATCH /status endpoints
    /// so the frontend can send { "status": "Approved" } instead of a raw enum string.
    /// </summary>
    public class UpdateStatusBodyDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}
