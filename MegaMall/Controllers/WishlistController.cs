using MegaMall.Domain.Entities;
using MegaMall.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MegaMall.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly MallDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public WishlistController(MallDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var items = await _context.WishlistItems
                .Include(w => w.Product)
                .ThenInclude(p => p.Variants)
                .Include(w => w.Product)
                .ThenInclude(p => p.Images)
                .Where(w => w.UserId == user.Id)
                .ToListAsync();

            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false, message = "User not found" });

            // Check if product exists
            var productExists = await _context.Products.AnyAsync(p => p.Id == productId);
            if (!productExists)
            {
                return Json(new { success = false, message = "Product not found" });
            }

            var existingItem = await _context.WishlistItems
                .FirstOrDefaultAsync(w => w.UserId == user.Id && w.ProductId == productId);

            if (existingItem != null)
            {
                _context.WishlistItems.Remove(existingItem);
            }
            else
            {
                _context.WishlistItems.Add(new WishlistItem
                {
                    UserId = user.Id,
                    ProductId = productId
                });
            }

            await _context.SaveChangesAsync();
            
            // Get updated count
            var count = await _context.WishlistItems
                .Where(w => w.UserId == user.Id)
                .CountAsync();

            return Json(new { success = true, count = count });
        }

        [HttpGet]
        public async Task<IActionResult> Count()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { count = 0 });

            var count = await _context.WishlistItems
                .Where(w => w.UserId == user.Id)
                .CountAsync();

            return Json(new { count });
        }

        [HttpGet]
        public async Task<IActionResult> GetWishlistProductIds()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { productIds = new int[] { } });

            var productIds = await _context.WishlistItems
                .Where(w => w.UserId == user.Id)
                .Select(w => w.ProductId)
                .ToListAsync();

            return Json(new { productIds });
        }
    }
}
