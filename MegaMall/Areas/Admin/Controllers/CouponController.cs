using MegaMall.Domain.Entities;
using MegaMall.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace MegaMall.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CouponController : Controller
    {
        private readonly MallDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CouponController(MallDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var coupons = await _context.Coupons.OrderByDescending(c => c.CreatedDate).ToListAsync();
            return View(coupons);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Coupon coupon)
        {
            // Validate
            if (await _context.Coupons.AnyAsync(c => c.Code == coupon.Code))
            {
                ModelState.AddModelError("Code", "Mã voucher này đã tồn tại");
            }

            if (coupon.Type == Domain.Enums.CouponType.Percentage)
            {
                if (coupon.DiscountValue < 0 || coupon.DiscountValue > 100)
                {
                    ModelState.AddModelError("DiscountValue", "Phần trăm giảm giá phải từ 0-100");
                }
            }

            if (coupon.ExpiryDate <= DateTime.Now)
            {
                ModelState.AddModelError("ExpiryDate", "Ngày hết hạn phải sau ngày hiện tại");
            }

            // Handle invalid/empty StartDate from binding
            // Verify if there is an error specifically for StartDate
            if (ModelState.ContainsKey("StartDate") && ModelState["StartDate"].Errors.Count > 0)
            {
                // If the user didn't input a valid date, StartDate will be MinValue
                // regardless, we want to default to Now and clear errors
                coupon.StartDate = DateTime.Now;
                ModelState.Remove("StartDate");
            }
            else if (coupon.StartDate == DateTime.MinValue)
            {
                // Should not happen if bound correctly without error, but for safety
                coupon.StartDate = DateTime.Now;
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                coupon.CreatedBy = user?.Id;
                coupon.CreatedDate = DateTime.Now;
                coupon.UsedCount = 0;

                // StartDate is not nullable, so no need to check
                // if not set from form, it defaults to DateTime.Now

                _context.Add(coupon);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Tạo voucher {coupon.Code} thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(coupon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon != null)
            {
                _context.Coupons.Remove(coupon);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã xóa voucher {coupon.Code}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
