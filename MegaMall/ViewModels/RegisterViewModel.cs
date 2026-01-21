using System.ComponentModel.DataAnnotations;

namespace MegaMall.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Display(Name = "Register as Seller?")]
        public bool IsSeller { get; set; }

        [Display(Name = "Shop Name (if seller)")]
        public string ShopName { get; set; }
    }
}
