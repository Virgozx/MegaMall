using MegaMall.Domain.Entities;
using MegaMall.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MegaMall.Controllers
{
    public class NewsletterController : Controller
    {
        private readonly MallDbContext _context;

        public NewsletterController(MallDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Subscribe(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
            {
                return Json(new { success = false, message = "Invalid email address." });
            }

            var existing = await _context.NewsletterSubscribers
                .FirstOrDefaultAsync(n => n.Email == email);

            if (existing != null)
            {
                if (!existing.IsActive)
                {
                    existing.IsActive = true;
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Welcome back! You have been resubscribed." });
                }
                return Json(new { success = false, message = "You are already subscribed." });
            }

            var subscriber = new NewsletterSubscriber { Email = email };
            _context.NewsletterSubscribers.Add(subscriber);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Thank you for subscribing!" });
        }
    }
}
