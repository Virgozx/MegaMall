using System.ComponentModel.DataAnnotations;

namespace MegaMall.Domain.Entities
{
    public class NewsletterSubscriber
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public DateTime SubscribedDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}
