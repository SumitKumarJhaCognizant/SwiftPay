using SwiftPay.Constants.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SwiftPay.Models;
using SwiftPay.Domain.Remittance.Entities;

namespace SwiftPay.Models
{
	public class RemitValidation
	{
		[Key]
		public Guid ValidationId { get; set; } = Guid.NewGuid();

		// Foreign Key to the Remittance
		public int RemitId { get; set; }

		[ForeignKey("RemitId")]
		public virtual RemittanceRequest RemittanceRequest { get; set; }

		//public string ValidationType { get; set; } // e.g., "KYC", "Limit", "FX_Quote"

		public SwiftPay.Constants.Enums.ValidationResult Result { get; set; } // Enum: Pass, Fail, Warning

		public ValidationRuleName RuleName { get; set; }

		public string Message { get; set; } // e.g., "Document verified successfully"

		public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
		public DateTime? UpdateDate { get; set; }

		public DateTime? CheckedDate { get; set; } // Add this property

		public bool IsDeleted { get; set; } // Add this property
	}
}