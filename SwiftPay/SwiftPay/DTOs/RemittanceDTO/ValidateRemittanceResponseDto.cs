namespace SwiftPay.DTOs.RemittanceDTO
{
	public class ValidateRemittanceResponseDto
	{
		public int RemitId { get; set; }
		public string Status { get; set; } = default!;

		// --- FINANCIAL SUMMARY FIELDS ---
		public decimal SendAmount { get; set; }
		public decimal? ReceiverAmount { get; set; }
		public decimal? RateApplied { get; set; }
		public decimal? FeeApplied { get; set; }
		public string? FromCurrency { get; set; }
		public string? ToCurrency { get; set; }
		// -------------------------------------

		public List<RemitValidationDto> Validations { get; set; } = new();
	}
}