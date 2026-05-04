using System.ComponentModel.DataAnnotations;

namespace SwiftPay.DTOs.RemittanceDTO
{
	public class CreateRemittanceDto
	{
		// --- 1. THE WHO ---
		[Required]
		public int CustomerId { get; set; }

		[Required]
		public int BeneficiaryId { get; set; }


		// --- 2. THE MONEY & RATE ---
		[Required]
		[Range(1, 1000000)]
		public decimal SendAmount { get; set; }

		[Required]
		public string? QuoteId { get; set; }


		// --- 3. THE COMPLIANCE ---
		[Required]
		[MaxLength(100)]
		public string PurposeCode { get; set; }

		[Required]
		[MaxLength(100)]
		public string SourceOfFunds { get; set; }


		// --- 4. CURRENCIES ---
		[Required]
		[StringLength(3, MinimumLength = 3)]
		public string FromCurrency { get; set; } // e.g., "USD"

		[Required]
		[StringLength(3, MinimumLength = 3)]
		public string ToCurrency { get; set; } // e.g., "EUR"
	}
}
