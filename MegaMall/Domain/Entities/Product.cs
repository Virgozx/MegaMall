using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MegaMall.Domain.Enums;

namespace MegaMall.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(256)]
        public string Name { get; set; }

        public string Description { get; set; }

        // JSON column to store dynamic attributes like {"Material": "Cotton", "Origin": "Vietnam"}
        // This avoids the EAV (Entity-Attribute-Value) performance issues
        public string AttributesJson { get; set; }

        public bool IsPublished { get; set; }
        public bool IsDeleted { get; set; }
        
        /// <summary>
        /// Hành vi khi sản phẩm được bán hết
        /// </summary>
        public ProductSoldBehavior SoldBehavior { get; set; } = ProductSoldBehavior.ShowOutOfStock;

        public string SellerId { get; set; } // Foreign Key to User
        public ApplicationUser Seller { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<ProductVideo> Videos { get; set; } = new List<ProductVideo>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
