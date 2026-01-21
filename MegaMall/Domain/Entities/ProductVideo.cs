using System.ComponentModel.DataAnnotations;

namespace MegaMall.Domain.Entities
{
    public class ProductVideo
    {
        public int Id { get; set; }
        
        [Required]
        public string VideoUrl { get; set; }
        
        public string ThumbnailUrl { get; set; }
        
        public int DisplayOrder { get; set; }
        
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
