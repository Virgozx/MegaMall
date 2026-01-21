using MegaMall.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MegaMall.Domain.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int? ProductVariantId { get; set; }
        public ProductVariant ProductVariant { get; set; }

        // Snapshot data (in case product changes later)
        public string ProductName { get; set; }
        public string Sku { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }
        
        public OrderStatus Status { get; set; } = OrderStatus.PendingPayment;
    }
}
