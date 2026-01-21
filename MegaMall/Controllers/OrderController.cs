using MegaMall.Domain.Entities;
using MegaMall.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace MegaMall.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly MallDbContext _context;
        private readonly ILogger<OrderController> _logger;

        public OrderController(MallDbContext context, ILogger<OrderController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Order
        public async Task<IActionResult> Index()
        {
            
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // GET: Order/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpGet]
        public async Task<IActionResult> GetOrderNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Json(new { count = 0, orders = new List<object>() });

            // Get recent 5 orders
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Select(o => new
                {
                    o.Id,
                    OrderDate = o.OrderDate.ToString("dd/MM/yyyy"),
                    Status = o.Status.ToString(), // Or map to friendly name if needed
                    TotalAmount = o.TotalAmount
                })
                .ToListAsync();

            // Count orders that are not Cancelled or Delivered (Active orders)?
            // Or just total new notifications? The user asked for "how many orders".
            // Let's count "Active" orders: PendingPayment, Paid, Processing, Shipped.
            var activeCount = await _context.Orders
                .CountAsync(o => o.UserId == userId && 
                    o.Status != Domain.Enums.OrderStatus.Cancelled && 
                    o.Status != Domain.Enums.OrderStatus.Refunded &&
                    o.Status != Domain.Enums.OrderStatus.Delivered);

            return Json(new { count = activeCount, orders });
        }

        [HttpPost]
        public async Task<IActionResult> RequestCancel(int id, string reason)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            // Valid status for cancellation: PendingPayment, Paid, Processing
            if (order.Status == Domain.Enums.OrderStatus.PendingPayment ||
                order.Status == Domain.Enums.OrderStatus.Paid ||
                order.Status == Domain.Enums.OrderStatus.Processing)
            {
                order.Status = Domain.Enums.OrderStatus.CancellationRequested;
                order.CancelReason = reason;
                _context.Update(order);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã gửi yêu cầu hủy đơn hàng. Vui lòng chờ người bán xác nhận.";
            }
            else
            {
                TempData["Error"] = "Không thể hủy đơn hàng ở trạng thái này.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> RequestReturn(int id, string reason)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            // Valid status for return: Delivered
            if (order.Status == Domain.Enums.OrderStatus.Delivered)
            {
                order.Status = Domain.Enums.OrderStatus.ReturnRequested;
                order.ReturnReason = reason;
                _context.Update(order);
                await _context.SaveChangesAsync();
                 TempData["Success"] = "Đã gửi yêu cầu hoàn hàng/hoàn tiền. Vui lòng chờ người bán xác nhận.";
            }
            else
            {
                TempData["Error"] = "Chỉ có thể yêu cầu hoàn hàng khi đơn hàng đã giao thành công.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
