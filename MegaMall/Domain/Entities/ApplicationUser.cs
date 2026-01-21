using Microsoft.AspNetCore.Identity;

namespace MegaMall.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string? AvatarUrl { get; set; }
        
        // Seller specific properties
        public string ShopName { get; set; }
        public string ShopDescription { get; set; }
        public bool IsSellerApproved { get; set; }

        public int LoyaltyPoints { get; set; }
    }
}
