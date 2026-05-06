using System;

namespace SwiftPay.DTOs.RemittanceDTO
{
    public class CreateRemittanceResponseDto
    {
        public int RemitId { get; set; }
        public int CustomerId { get; set; }
        public int BeneficiaryId { get; set; }

        // Currency + amounts
        public string FromCurrency { get; set; } = string.Empty;
        public string ToCurrency { get; set; } = string.Empty;
        public decimal SendAmount { get; set; }          // primary field (was "Amount")
        public decimal Amount { get; set; }              // alias so old consumers still work
        public decimal? ReceiverAmount { get; set; }
        public decimal? RateApplied { get; set; }
        public decimal? FeeApplied { get; set; }

        // Quote linkage
        public string? QuoteId { get; set; }

        // Compliance / reporting fields
        public string? PurposeCode { get; set; }
        public string? SourceOfFunds { get; set; }

        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
