namespace SwiftPay.Constants.Enums
{
    // Keeping your naming style
    public enum RoutingRuleStatus
    {
        Inactive = 0,
        Active = 1,
        Suspended = 2
    }

    // Since RoutingRule also needs PayoutMode (matches spec: Account/CashPickup/MobileWallet)
    public enum PayoutModeStatus
    {
        Account = 1,
        CashPickup = 2,
        MobileWallet = 3
    }
}