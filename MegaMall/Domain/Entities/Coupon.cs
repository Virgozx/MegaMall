using System.ComponentModel.DataAnnotations;
using MegaMall.Domain.Enums;

namespace MegaMall.Domain.Entities
{
    public class Coupon
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Code { get; set; }
        
        /// <summary>
        /// Tên/Mô tả voucher
        /// </summary>
        [MaxLength(256)]
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Loại voucher: Fixed Amount, Percentage, FreeShipping
        /// </summary>
        public CouponType Type { get; set; } = CouponType.FixedAmount;
        
        /// <summary>
        /// Giá trị giảm giá (số tiền hoặc % tùy theo Type)
        /// </summary>
        public decimal DiscountValue { get; set; }
        
        /// <summary>
        /// Giảm tối đa (chỉ áp dụng cho Percentage type)
        /// </summary>
        public decimal? MaxDiscountAmount { get; set; }
        
        /// <summary>
        /// Đơn hàng tối thiểu để áp dụng voucher
        /// </summary>
        public decimal MinOrderAmount { get; set; }
        
        /// <summary>
        /// Số lượng voucher có sẵn
        /// </summary>
        public int? Quantity { get; set; }
        
        /// <summary>
        /// Số lượng đã sử dụng
        /// </summary>
        public int UsedCount { get; set; } = 0;
        
        /// <summary>
        /// Số lần sử dụng tối đa cho mỗi user (null = không giới hạn)
        /// </summary>
        public int? MaxUsagePerUser { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime ExpiryDate { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Admin tạo voucher
        /// </summary>
        public string? CreatedBy { get; set; }
    }
}
