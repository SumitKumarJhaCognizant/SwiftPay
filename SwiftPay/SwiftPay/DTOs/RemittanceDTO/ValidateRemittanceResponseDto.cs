namespace SwiftPay.DTOs.RemittanceDTO
{
    public class ValidateRemittanceResponseDto
    {

        // RemitId is an integer in the domain model
        public int RemitId { get; set; }


		/// <summary>
		/// Current status of the remittance (Draft / Validated / ComplianceHold).
		/// </summary>
		public string Status { get; set; } = default!;

        /// <summary>
        /// List of validation results executed for the remittance.
        /// </summary>
        public List<RemitValidationDto> Validations { get; set; } = new();

    }
}
