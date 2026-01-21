using MegaMall.Domain.Enums;
using MegaMall.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MegaMall.Domain.Entities;
using MegaMall.Interfaces;

namespace MegaMall.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly MallDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICalendarService _calendarService;

        public HomeController(MallDbContext context, UserManager<ApplicationUser> userManager, ICalendarService calendarService)
        {
            _context = context;
            _userManager = userManager;
            _calendarService = calendarService;
        }

        public async Task<IActionResult> Index()
        {
            // 1. General Stats
            var totalSales = await _context.Orders
                .Where(o => o.Status != OrderStatus.Cancelled)
                .SumAsync(o => o.TotalAmount);

            var totalOrders = await _context.Orders.CountAsync();
            var totalUsers = await _context.Users.CountAsync();
            var totalProducts = await _context.Products.CountAsync();
            
            // Voucher Statistics
            var totalCoupons = await _context.Coupons.CountAsync();
            var activeCoupons = await _context.Coupons
                .Where(c => c.IsActive && c.ExpiryDate > DateTime.Now && c.UsedCount < (c.Quantity ?? int.MaxValue))
                .CountAsync();
            var totalCouponUsage = await _context.Coupons.SumAsync(c => c.UsedCount);
            var totalCouponDiscount = await _context.Orders
                .Where(o => !string.IsNullOrEmpty(o.CouponCode))
                .SumAsync(o => o.DiscountAmount);
            
            var topCoupons = await _context.Coupons
                .OrderByDescending(c => c.UsedCount)
                .Take(5)
                .ToListAsync();
            
            var recentOrders = await _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Include(o => o.User)
                .ToListAsync();

            // 2. Sales Chart (Last 7 days)
            var today = DateTime.Today;
            var startDate = today.AddDays(-6);
            
            var salesDataRaw = await _context.Orders
                .Where(o => o.OrderDate >= startDate && o.Status != OrderStatus.Cancelled)
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new { Date = g.Key, Total = g.Sum(o => o.TotalAmount) })
                .ToListAsync();

            var salesData = new List<decimal>();
            var labels = new List<string>();

            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var dailySale = salesDataRaw.FirstOrDefault(s => s.Date == date)?.Total ?? 0;
                
                salesData.Add(dailySale);
                labels.Add(date.ToString("dd/MM"));
            }

            // 3. Product Categories Distribution (Top 5 + Others)
            var categoryStats = await _context.Products
                .GroupBy(p => p.Category.Name)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            var catLabels = categoryStats.Take(5).Select(x => x.Category).ToList();
            var catData = categoryStats.Take(5).Select(x => x.Count).ToList();
            
            if (categoryStats.Count > 5)
            {
                catLabels.Add("Khác");
                catData.Add(categoryStats.Skip(5).Sum(x => x.Count));
            }

            // 4. Top Sellers by Product Count
            var topSellers = await _context.Products
                .Include(p => p.Seller)
                .GroupBy(p => p.Seller.ShopName ?? p.Seller.UserName)
                .Select(g => new { Seller = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            // 5. User Roles Distribution
            // Note: This is an approximation as users can have multiple roles, but usually 1 main role
            var adminCount = (await _userManager.GetUsersInRoleAsync("Admin")).Count;
            var sellerCount = (await _userManager.GetUsersInRoleAsync("Seller")).Count;
            var buyerCount = totalUsers - adminCount - sellerCount; // Rough estimate or query directly if needed

            // 6. Get Upcoming Vietnamese Holidays
            var upcomingHolidays = await _calendarService.GetUpcomingVietnameseHolidaysAsync(3);

            ViewBag.TotalSales = totalSales;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalProducts = totalProducts;
            ViewBag.RecentOrders = recentOrders;
            
            // Voucher ViewBags
            ViewBag.TotalCoupons = totalCoupons;
            ViewBag.ActiveCoupons = activeCoupons;
            ViewBag.TotalCouponUsage = totalCouponUsage;
            ViewBag.TotalCouponDiscount = totalCouponDiscount;
            ViewBag.TopCoupons = topCoupons;
            
            // Chart Data Bags
            ViewBag.SalesLabels = labels;
            ViewBag.SalesData = salesData;

            ViewBag.CatLabels = catLabels;
            ViewBag.CatData = catData;

            ViewBag.SellerLabels = topSellers.Select(x => x.Seller).ToList();
            ViewBag.SellerData = topSellers.Select(x => x.Count).ToList();

            ViewBag.RoleLabels = new List<string> { "Admin", "Seller", "Buyer" };
            ViewBag.RoleData = new List<int> { adminCount, sellerCount, buyerCount };
            
            ViewBag.UpcomingHolidays = upcomingHolidays;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ExportReport()
        {
            // Gather Data
            var totalSales = await _context.Orders
                .Where(o => o.Status != OrderStatus.Cancelled)
                .SumAsync(o => o.TotalAmount);

            var orders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(1000)
                .ToListAsync();

            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .OrderByDescending(p => p.CreatedDate)
                .Take(1000)
                .ToListAsync();

            // Build CSV with BOM for Excel compatibility
            var csv = new System.Text.StringBuilder();
            // Start with BOM
            csv.Append('\uFEFF'); 

            // 1. Summary
            csv.AppendLine("BÁO CÁO TỔNG QUAN HỆ THỐNG MEGAMALL");
            csv.AppendLine($"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            csv.AppendLine($"Tổng doanh thu: {totalSales:N0}");
            csv.AppendLine($"Tổng số đơn hàng: {await _context.Orders.CountAsync()}");
            csv.AppendLine($"Tổng số thành viên: {await _context.Users.CountAsync()}");
            csv.AppendLine($"Tổng số sản phẩm: {await _context.Products.CountAsync()}");
            csv.AppendLine();

            // 2. Orders
            csv.AppendLine("DANH SÁCH ĐƠN HÀNG GẦN ĐÂY (Top 1000)");
            csv.AppendLine("Mã Đơn,Ngày Đặt,Khách Hàng,Tổng Tiền,Trạng Thái,Phương Thức TT");
            foreach (var order in orders)
            {
                var customerName = order.User?.FullName ?? "Khách vãng lai";
                // Escape commas in names
                customerName = "\"" + customerName.Replace("\"", "\"\"") + "\"";
                var status = order.Status.ToString();
                csv.AppendLine($"{order.Id},{order.OrderDate:dd/MM/yyyy HH:mm},{customerName},{order.TotalAmount:F0},{status},{order.PaymentMethod}");
            }
            csv.AppendLine();

            // 3. Products
            csv.AppendLine("DANH SÁCH SẢN PHẨM MỚI (Top 1000)");
            csv.AppendLine("ID,Tên Sản Phẩm,Danh Mục,Người Bán,Ngày Tạo");
            foreach (var prod in products)
            {
                var category = prod.Category?.Name ?? "N/A";
                var seller = prod.Seller?.ShopName ?? prod.Seller?.UserName ?? "N/A";
                
                var name = "\"" + prod.Name.Replace("\"", "\"\"") + "\"";
                category = "\"" + category.Replace("\"", "\"\"") + "\"";
                seller = "\"" + seller.Replace("\"", "\"\"") + "\"";

                csv.AppendLine($"{prod.Id},{name},{category},{seller},{prod.CreatedDate:dd/MM/yyyy}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            var result = new FileContentResult(bytes, "text/csv");
            result.FileDownloadName = $"BaoCao_MegaMall_{DateTime.Now:yyyyMMddHHmm}.csv";
            return result;
        }
    }
}
