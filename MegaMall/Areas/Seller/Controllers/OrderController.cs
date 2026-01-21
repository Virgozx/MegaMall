using MegaMall.Domain.Entities;
using MegaMall.Domain.Enums;
using MegaMall.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MegaMall.Areas.Seller.Controllers
{
    [Area("Seller")]
    [Authorize(Roles = "Seller")]
    public class OrderController : Controller
    {
        private readonly MallDbContext _context;

        public OrderController(MallDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Find OrderItems belonging to this seller
            var orderItems = await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                .Where(oi => oi.Product.SellerId == sellerId)
                .OrderByDescending(oi => oi.Order.OrderDate)
                .ToListAsync();

            // Group by Order for display
            var groupedOrders = orderItems
                .GroupBy(oi => oi.Order)
                .Select(g => new SellerOrderViewModel
                {
                    Order = g.Key,
                    Items = g.ToList()
                })
                .ToList();

            return View(groupedOrders);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int orderItemId, OrderStatus newStatus)
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var item = await _context.OrderItems
                .Include(oi => oi.Product)
                .FirstOrDefaultAsync(oi => oi.Id == orderItemId && oi.Product.SellerId == sellerId);

            if (item == null)
            {
                return NotFound();
            }

            item.Status = newStatus;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }

    public class SellerOrderViewModel
    {
        public Order Order { get; set; }
        public List<OrderItem> Items { get; set; }
    }
}
