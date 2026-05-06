namespace SwiftPay.Constants.Enums
{
    public enum FXQuoteStatus
    {
        Active = 0,
        Expired = 1,
        // --- ADDED: The missing status for successfully used quotes ---
        Locked = 2 
    }
}