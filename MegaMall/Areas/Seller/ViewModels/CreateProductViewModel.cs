using System.ComponentModel.DataAnnotations;
using MegaMall.Domain.Enums;

namespace MegaMall.Areas.Seller.ViewModels
{
    public class CreateProductViewModel
    {
        [Required]
        [MaxLength(256)]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }
        
        // Simplified for MVP: Just one attribute input
        [Display(Name = "Material (Optional)")]
        public string Material { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Display(Name = "Product Images")]
        public List<IFormFile> Images { get; set; }
        
        [Display(Name = "Product Videos")]
        public List<IFormFile> Videos { get; set; }
        
        [Display(Name = "Hành vi khi bán hết")]
        public ProductSoldBehavior SoldBehavior { get; set; } = ProductSoldBehavior.ShowOutOfStock;

        // Variants
        public List<ProductVariantViewModel> Variants { get; set; } = new List<ProductVariantViewModel>();
    }

    public class ProductVariantViewModel
    {
        [Required]
        [Display(Name = "SKU")]
        public string Sku { get; set; }

        [Required]
        [Display(Name = "Giá")]
        public decimal Price { get; set; }

        [Display(Name = "Giá gốc")]
        public decimal? OriginalPrice { get; set; }

        [Required]
        [Display(Name = "Số lượng")]
        public int StockQuantity { get; set; }

        [Display(Name = "Màu sắc")]
        public string Color { get; set; }

        [Display(Name = "Kích thước")]
        public string Size { get; set; }

        [Display(Name = "Thuộc tính khác")]
        public string OtherAttribute { get; set; }
    }
}
