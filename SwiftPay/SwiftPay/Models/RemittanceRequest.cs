using System;
//using SwiftPay.Domain.Remittance.Enums;

namespace SwiftPay.Domain.Remittance.Entities
{
	/// <summary>
	/// Remittance request header.
	/// </summary>
	public class RemittanceRequest
	{
		public string RemitId { get; set; }

		// Foreign references (by ID only for Phase-1)
		public string CustomerId { get; set; }

		public string BeneficiaryId { get; set; }

		// Currencies (ISO 4217)
		public string FromCurrency { get; set; }

		public string ToCurrency { get; set; }

		// Amounts
		public decimal SendAmount { get; set; }

		public decimal? ReceiverAmount { get; set; }

		// FX & Fees linkage
		public string? QuoteId { get; set; }

		public string? RateLockId { get; set; }

		public decimal? RateApplied { get; set; }

		public decimal? FeeApplied { get; set; }

		// Purpose & SoF
		public string? PurposeCode { get; set; }

		public string? SourceOfFunds { get; set; }

		// Status & lifecycle
		//public RemittanceStatus Status { get; set; } = RemittanceStatus.Draft;

		public DateTimeOffset CreatedDate { get; set; }

		public string PurposeCode { get; set; }      // required, non-empty
		public string SourceOfFunds { get; set; }    // required, non-empty

		// Audit
		public string? CreatedByUserId { get; set; }

		public string? UpdatedByUserId { get; set; }

		// Optimistic concurrency
		public byte[] RowVersion { get; set; }
	}
}
