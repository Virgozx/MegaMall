using MegaMall.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MegaMall.Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public string CouponCode { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public OrderStatus Status { get; set; }

        [Required]
        public string ShippingAddress { get; set; }

        [Required]
        public string ShippingCity { get; set; }

        public string PhoneNumber { get; set; }

        public string Note { get; set; }

        public string FullName { get; set; }
        
        public string PaymentStatus { get; set; }
        
        public DateTime? PaymentDate { get; set; }
        
        public string TransactionId { get; set; }

        public string? CancelReason { get; set; }
        public string? ReturnReason { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
