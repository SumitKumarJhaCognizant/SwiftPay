namespace SwiftPay.DTOs.RemittanceDTO
{
    public class RemitValidationDto
    {
        // ValidationId corresponds to RemitValidation.ValidationId
        public Guid ValidationId { get; set; }

        // RemitId is an integer in the domain model
        public int RemitId { get; set; }

        public string Rule { get; set; } = default!;
        public string Result { get; set; } = default!;
        public string? Message { get; set; }
        public DateTimeOffset CheckedDate { get; set; }
        public bool IsDeleted { get; set; } // Added property
    }
}
