using System.ComponentModel.DataAnnotations;

namespace MegaMall.Domain.Entities
{
    public class UserAddress
    {
        public int Id { get; set; }
        
        [Required]
        public string AddressLine { get; set; }
        
        [Required]
        public string City { get; set; }
        
        public bool IsDefault { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
