using System.ComponentModel.DataAnnotations;
using MegaMall.Domain.Enums;

namespace MegaMall.ViewModels
{
    public class CheckoutViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Required]
        [Display(Name = "City")]
        public string City { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Note")]
        public string? Note { get; set; }

        [Display(Name = "Coupon Code")]
        public string? CouponCode { get; set; }

        public decimal DiscountAmount { get; set; }

        [Required]
        [Display(Name = "Payment Method")]
        public PaymentMethod PaymentMethod { get; set; }

        // Mock Payment Fields
        [Display(Name = "Card Number")]
        public string CardNumber { get; set; }

        [Display(Name = "Card Holder Name")]
        public string CardHolderName { get; set; }

        [Display(Name = "Expiry Date (MM/YY)")]
        public string ExpiryDate { get; set; }

        [Display(Name = "CVV")]
        public string CVV { get; set; }

        // Loyalty Points
        public bool UseLoyaltyPoints { get; set; }
        public int AvailablePoints { get; set; }
        public decimal PointsDiscount { get; set; }

        public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();
        
        public decimal TotalAmount => CartItems.Sum(x => x.Total);
        
        public decimal FinalAmount => TotalAmount - DiscountAmount - PointsDiscount;
    }
}
