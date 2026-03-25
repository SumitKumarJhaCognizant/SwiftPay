using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SwiftPay.Constants.Enums;
using SwiftPay.Domain.Remittance.Entities;
namespace SwiftPay.Models
{
    public class RemitValidation
    {

		public Guid ValidationId { get; set; }       // PK

        public int RemitId { get; set; }            // FK -> RemittanceRequest (int)

        [ForeignKey("RemitId")]
        [System.ComponentModel.DataAnnotations.Schema.InverseProperty("Validations")]
        public virtual RemittanceRequest RemittanceRequest { get; set; }

		public ValidationRuleName RuleName { get; set; }   // Limit/Velocity/Docs/Corridor
		public Constants.Enums.ValidationResult Result { get; set; }       // Pass/Fail

		public string Message { get; set; }          // required, non-empty
		public DateTimeOffset CheckedDate { get; set; } // default GETUTCDATE()

		public DateTime CreatedDate { get; set; }
		public DateTime UpdateDate { get; set; }

		public bool IsDeleted { get; set; }

	}
}
