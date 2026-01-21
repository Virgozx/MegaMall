namespace MegaMall.Domain.Enums
{
    /// <summary>
    /// Hành vi của sản phẩm khi được bán
    /// </summary>
    public enum ProductSoldBehavior
    {
        /// <summary>
        /// Hiển thị hết hàng - Sản phẩm vẫn hiển thị nhưng đánh dấu hết hàng
        /// </summary>
        ShowOutOfStock = 0,
        
        /// <summary>
        /// Tự động ẩn - Sản phẩm sẽ tự động bị ẩn khỏi trang web
        /// </summary>
        AutoHide = 1
    }
}
