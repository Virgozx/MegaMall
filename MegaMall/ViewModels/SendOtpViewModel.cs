using System.ComponentModel.DataAnnotations;

namespace MegaMall.ViewModels
{
    public class SendOtpViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;
    }
}
