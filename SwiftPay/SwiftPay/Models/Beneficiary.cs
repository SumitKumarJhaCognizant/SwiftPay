using System;
using Model;
using SwiftPay.Constants.Enums;

namespace SwiftPay.Domain.Remittance.Entities
{
    public class Beneficiary
    {
        public int BeneficiaryID { get; set; }

        // foreign key to CustomerProfile
        public int CustomerID { get; set; }

        public string Name { get; set; }

        public string Country { get; set; }

        public PayoutMode PayoutMode { get; set; }

        public string BankName { get; set; }

        public string BankCountry { get; set; }

        public string AccountOrWalletNo { get; set; }

        public string IFSC_IBAN_SWIFT { get; set; }

        public BeneficiaryVerificationStatus VerificationStatus { get; set; }

        public DateTime AddedDate { get; set; }

        public BeneficiaryStatus Status { get; set; }

        // navigation
        public CustomerProfile Customer { get; set; }
    }
}