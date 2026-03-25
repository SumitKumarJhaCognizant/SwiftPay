using System.Xml.Linq;

using SwiftPay.Constants.Enums;
namespace SwiftPay.Models
{
    public class Document
    {
		
		public int DocumentId { get; set; }         // PK

        public int RemitId { get; set; }            // FK -> RemittanceRequest (int)

        [System.ComponentModel.DataAnnotations.Schema.ForeignKey("RemitId")]
        [System.ComponentModel.DataAnnotations.Schema.InverseProperty("Documents")]
        public virtual Domain.Remittance.Entities.RemittanceRequest RemittanceRequest { get; set; }

		public DocumentType DocType { get; set; }    // IDProof/SoF/Invoice/Declaration
		public string FileURI { get; set; }          // required, non-empty

		public DateTimeOffset UploadedDate { get; set; } // default GETUTCDATE()
		public VerificationStatus VerificationStatus { get; set; } // default Pending

		public DateTime CreatedDate { get; set; }
		public DateTime UpdateDate { get; set; }

		public bool IsDeleted { get; set; }
		

	}
}
