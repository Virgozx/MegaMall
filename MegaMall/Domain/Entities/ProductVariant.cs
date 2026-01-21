using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MegaMall.Domain.Entities
{
    public class ProductVariant
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Required]
        [MaxLength(50)]
        public string Sku { get; set; } // Stock Keeping Unit, e.g., "SHIRT-RED-L"

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? OriginalPrice { get; set; }

        public int StockQuantity { get; set; }

        // JSON column for specific variant properties like {"Color": "Red", "Size": "L"}
        public string VariantPropertiesJson { get; set; }
    }
}
