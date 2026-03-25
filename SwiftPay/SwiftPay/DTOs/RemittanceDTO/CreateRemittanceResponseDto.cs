using System;

namespace SwiftPay.DTOs.RemittanceDTO
{
    public class CreateRemittanceResponseDto
    {
        // RemitId stored as integer in the domain model
        public int RemitId { get; set; }
        public int CustomerId { get; set; } // Added property
        public decimal Amount { get; set; } // Added property

        public int BeneficiaryId { get; set; } // Added property
		public string Status { get; set; } = default!;

        public string FromCurrency { get; set; } = default!;
        public string ToCurrency { get; set; } = default!;

        public decimal? ReceiverAmount { get; set; }

        public decimal? RateApplied { get; set; }
        public decimal? FeeApplied { get; set; }

        public DateTime CreatedDate { get; set; }

        public bool IsDeleted { get; set; } // Added property
    }
}
