namespace MegaMall.Domain.Enums
{
    /// <summary>
    /// Loại voucher/coupon
    /// </summary>
    public enum CouponType
    {
        /// <summary>
        /// Giảm giá theo số tiền cố định (VNĐ)
        /// </summary>
        FixedAmount = 0,
        
        /// <summary>
        /// Giảm giá theo phần trăm (%)
        /// </summary>
        Percentage = 1,
        
        /// <summary>
        /// Miễn phí vận chuyển
        /// </summary>
        FreeShipping = 2
    }
}
