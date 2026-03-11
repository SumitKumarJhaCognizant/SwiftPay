using System.ComponentModel.DataAnnotations;
using RemittanceModule;
using SwiftPay.Constants.Enums;
namespace SwiftPay.Models
{
    public class RemitValidation
    {

		public Guid ValidationId { get; set; }       // PK

		public Guid RemitId { get; set; }            // FK -> RemittanceRequest
		public virtual RemittanceRequest RemittanceRequest { get; set; }

		public ValidationRuleName RuleName { get; set; }   // Limit/Velocity/Docs/Corridor
		public ValidationResult Result { get; set; }       // Pass/Fail

		public string Message { get; set; }          // required, non-empty
		public DateTimeOffset CheckedDate { get; set; } // default GETUTCDATE()

		//validations

	}
}
