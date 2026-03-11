using System.Xml.Linq;
using RemittanceModule;
using SwiftPay.Constants.Enums;
namespace SwiftPay.Models
{
    public class Document
    {

		public int DocumentId { get; set; }         // PK

		public int RemitId { get; set; }            // FK -> RemittanceRequest
		public virtual RemittanceRequest RemittanceRequest { get; set; }

		public DocumentType DocType { get; set; }    // IDProof/SoF/Invoice/Declaration
		public string FileURI { get; set; }          // required, non-empty

		public DateTimeOffset UploadedDate { get; set; } // default GETUTCDATE()
		public VerificationStatus VerificationStatus { get; set; } // default Pending

	}
}
