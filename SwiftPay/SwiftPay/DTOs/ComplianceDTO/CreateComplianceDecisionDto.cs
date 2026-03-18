

using SwiftPay.Constants.Enums;
using SwiftPay.DTOs.ComplianceDTO; // <--- Make sure this is exactly like this
using System.ComponentModel.DataAnnotations;

namespace SwiftPay.DTOs.ComplianceDTO
{
    public class CreateComplianceDecisionDto
    {
        [Required] public string RemitId { get; set; }
        [Required] public string AnalystId { get; set; }
        [Required] public string Decision { get; set; } // Approve, Hold, Reject
        public string? Notes { get; set; }
    }

    public class UpdateComplianceDecisionDto
    {
        [Required]
        public ComplianceDecisionStatus Decision { get; set; }
        public string? Notes { get; set; }
    }
}