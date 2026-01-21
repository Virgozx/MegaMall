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
        Refunded
    }
}
