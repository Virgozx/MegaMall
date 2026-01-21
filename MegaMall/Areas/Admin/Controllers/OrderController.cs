using MegaMall.Domain.Entities;
using MegaMall.Domain.Enums;
using MegaMall.Data;
using MegaMall.Interfaces;
using MegaMall.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace MegaMall.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly MallDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IEmailService _emailService;

        public OrderController(MallDbContext context, IHubContext<NotificationHub> hubContext, IEmailService emailService)
        {
            _context = context;
            _hubContext = hubContext;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                order.Status = status;
                await _context.SaveChangesAsync();

                // Notify User
                await _hubContext.Clients.User(order.UserId).SendAsync("ReceiveNotification", $"Your order #{order.Id} status has been updated to {status}.");
                
                // Send Email
                var user = await _context.Users.FindAsync(order.UserId);
                if (user != null)
                {
                    await _emailService.SendEmailAsync(user.Email, $"Order Update #{order.Id}", $"Your order status is now: {status}");
                }
            }
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
