using MegaMall.Domain.Entities;
using MegaMall.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace MegaMall.Controllers
{
    [Authorize] // Require login for all actions
    public class ReviewController : Controller
    {
        private readonly MallDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReviewController(MallDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId, int rating, string comment)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                // Validate input
                if (rating < 1 || rating > 5)
                {
                    TempData["Error"] = "Đánh giá phải từ 1 đến 5 sao";
                    return RedirectToAction("Details", "Product", new { id = productId });
                }

                if (string.IsNullOrWhiteSpace(comment))
                {
                    TempData["Error"] = "Vui lòng nhập nhận xét";
                    return RedirectToAction("Details", "Product", new { id = productId });
                }

                // Check if product exists
                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    TempData["Error"] = "Sản phẩm không tồn tại";
                    return RedirectToAction("Index", "Home");
                }

                // Check if user already reviewed this product
                var existingReview = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.ProductId == productId && r.UserId == user.Id);

                if (existingReview != null)
                {
                    // Update existing review
                    existingReview.Rating = rating;
                    existingReview.Comment = comment.Trim();
                    existingReview.CreatedDate = DateTime.Now;
                    
                    TempData["Success"] = "Đánh giá của bạn đã được cập nhật!";
                }
                else
                {
                    // Create new review
                    var review = new Review
                    {
                        ProductId = productId,
                        UserId = user.Id,
                        Rating = rating,
                        Comment = comment.Trim(),
                        CreatedDate = DateTime.Now
                    };

                    _context.Reviews.Add(review);
                    TempData["Success"] = "Cảm ơn bạn đã đánh giá sản phẩm!";
                }

                await _context.SaveChangesAsync();

                return RedirectToAction("Details", "Product", new { id = productId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi gửi đánh giá. Vui lòng thử lại sau.";
                return RedirectToAction("Details", "Product", new { id = productId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                var review = await _context.Reviews.FindAsync(id);
                if (review == null)
                {
                    return NotFound();
                }

                // Only allow user to delete their own review or admin
                if (review.UserId != user.Id && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var productId = review.ProductId;
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Đã xóa đánh giá";
                return RedirectToAction("Details", "Product", new { id = productId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi xóa đánh giá";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
