using MegaMall.Domain.Entities;
using MegaMall.Data;
using MegaMall.Areas.Seller.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MegaMall.Areas.Seller.Controllers
{
    [Area("Seller")]
    [Authorize(Roles = "Seller")]
    public class DashboardController : Controller
    {
        private readonly MallDbContext _context;

        public DashboardController(MallDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var orderItems = await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                .Where(oi => oi.Product.SellerId == sellerId)
                .OrderByDescending(oi => oi.Order.OrderDate)
                .ToListAsync();

            // Get product statistics
            var totalProducts = await _context.Products
                .Where(p => p.SellerId == sellerId && !p.IsDeleted)
                .CountAsync();

            var publishedProducts = await _context.Products
                .Where(p => p.SellerId == sellerId && !p.IsDeleted && p.IsPublished)
                .CountAsync();

            var totalStock = await _context.ProductVariants
                .Where(v => v.Product.SellerId == sellerId && !v.Product.IsDeleted)
                .SumAsync(v => v.StockQuantity);

            var viewModel = new SellerDashboardViewModel
            {
                TotalOrders = orderItems.Count,
                TotalRevenue = orderItems.Sum(oi => oi.Quantity * oi.UnitPrice),
                RecentOrderItems = orderItems.Take(5).ToList()
            };

            // Pass product stats to ViewBag
            ViewBag.TotalProducts = totalProducts;
            ViewBag.PublishedProducts = publishedProducts;
            ViewBag.TotalStock = totalStock;

            // Prepare Chart Data (Revenue by Day for last 7 days)
            var last7Days = Enumerable.Range(0, 7).Select(i => DateTime.Today.AddDays(-i)).Reverse().ToList();
            var chartData = new List<decimal>();
            var chartLabels = new List<string>();

            foreach (var day in last7Days)
            {
                var revenue = orderItems
                    .Where(oi => oi.Order.OrderDate.Date == day)
                    .Sum(oi => oi.Quantity * oi.UnitPrice);
                chartData.Add(revenue);
                chartLabels.Add(day.ToString("dd/MM"));
            }

            ViewBag.ChartLabels = chartLabels;
            ViewBag.ChartData = chartData;

            return View(viewModel);
        }
    }
}
