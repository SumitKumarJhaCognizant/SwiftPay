namespace SwiftPay.Constants.Enums
{
    public enum KYCLevel
    {
        Min,
        Full,
        Enhanced
    }

    // renamed to avoid conflict with existing VerificationStatus in Document.cs
    public enum KycVerificationStatus
    {
        Pending,
        Verified,
        Rejected
    }
}