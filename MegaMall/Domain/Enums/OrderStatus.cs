namespace MegaMall.Domain.Enums
{
    public enum OrderStatus
    {
        PendingPayment,
        Paid,
        Processing,
        Shipped,
        Delivered,
        Cancelled,
        Refunded,
        CancellationRequested, // 7
        ReturnRequested        // 8
    }
}
