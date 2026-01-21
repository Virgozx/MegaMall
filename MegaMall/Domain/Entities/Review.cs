using System.ComponentModel.DataAnnotations;

namespace MegaMall.Domain.Entities
{
    public class Review
    {
        public int Id { get; set; }
        
        [Range(1, 5)]
        public int Rating { get; set; }
        
        [Required]
        public string Comment { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
