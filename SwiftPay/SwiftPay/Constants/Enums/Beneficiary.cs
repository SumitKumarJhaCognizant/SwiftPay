namespace SwiftPay.Constants.Enums
{
    public enum PayoutMode
    {
        Account,
        CashPickup,
        MobileWallet
    }

    public enum BeneficiaryVerificationStatus
    {
        Pending,
        Verified,
        Rejected
    }

    public enum BeneficiaryStatus
    {
        Active,
        Inactive,
        Suspended
    }
}