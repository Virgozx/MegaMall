using MegaMall.Domain.Entities;
using MegaMall.Domain.Enums;
using MegaMall.Data;
using MegaMall.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

using MegaMall.Interfaces;
using MegaMall.Hubs;
using Microsoft.AspNetCore.SignalR;
using VNPAY;

namespace MegaMall.Controllers
{
    public class AddToCartRequest
    {
        public int ProductId { get; set; }
        public int VariantId { get; set; }
        public int Quantity { get; set; }
    }

    public class CartController : Controller
    {
        private readonly MallDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IEmailService _emailService;
        private readonly IVnpayClient _vnpayClient;
        private const string CartSessionKey = "CartSession";
        private const string CouponSessionKey = "CouponSession";

        public CartController(
            MallDbContext context, 
            UserManager<ApplicationUser> userManager, 
            IHubContext<NotificationHub> hubContext, 
            IEmailService emailService,
            IVnpayClient vnpayClient)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
            _emailService = emailService;
            _vnpayClient = vnpayClient;
        }

        [Authorize]
        public IActionResult Index()
        {
            var cart = GetCartFromSession();
            return View(cart);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(string couponCode)
        {
            if (string.IsNullOrEmpty(couponCode))
            {
                return Json(new { success = false, message = "Vui lòng nhập mã giảm giá" });
            }

            var coupon = await _context.Coupons
                .FirstOrDefaultAsync(c => c.Code == couponCode && c.IsActive);

            if (coupon == null)
            {
                return Json(new { success = false, message = "Mã giảm giá không tồn tại" });
            }

            // Check expiry
            if (coupon.ExpiryDate <= DateTime.Now)
            {
                return Json(new { success = false, message = "Mã giảm giá đã hết hạn" });
            }

            // Check start date
            if (coupon.StartDate > DateTime.Now)
            {
                return Json(new { success = false, message = $"Mã giảm giá sẽ có hiệu lực từ {coupon.StartDate:dd/MM/yyyy}" });
            }

            // Check quantity
            if (coupon.UsedCount >= coupon.Quantity)
            {
                return Json(new { success = false, message = "Mã giảm giá đã hết lượt sử dụng" });
            }

            // Check per-user limit (optional - would need to track usage per user in a separate table)
            // For now, we skip this check

            var cart = GetCartFromSession();
            var total = cart.Sum(x => x.Total);

            if (total < coupon.MinOrderAmount)
            {
                return Json(new { success = false, message = $"Đơn hàng tối thiểu {coupon.MinOrderAmount:N0}đ để sử dụng mã này" });
            }

            HttpContext.Session.SetString(CouponSessionKey, coupon.Code);
            var discountAmount = CalculateCouponDiscount(coupon, total);

            string message = "Áp dụng mã giảm giá thành công";
            if (coupon.Type == Domain.Enums.CouponType.FreeShipping)
            {
                message = "Áp dụng mã miễn phí vận chuyển thành công";
            }
            else if (coupon.Type == Domain.Enums.CouponType.Percentage)
            {
                message = $"Áp dụng mã giảm {coupon.DiscountValue}% thành công";
            }

            return Json(new 
            { 
                success = true, 
                message = message,
                discount = discountAmount
            });
        }

        [Authorize]
        [HttpPost]
        public IActionResult RemoveCoupon()
        {
            HttpContext.Session.Remove(CouponSessionKey);
            TempData["Success"] = "Coupon removed.";
            return RedirectToAction(nameof(Checkout));
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("Cart/Add")]
        public async Task<IActionResult> Add([FromBody] AddToCartRequest request)
        {
            try
            {
                var variant = await _context.ProductVariants
                    .Include(v => v.Product)
                    .FirstOrDefaultAsync(v => v.Id == request.VariantId);

                if (variant == null) 
                    return Json(new { success = false, message = "Sản phẩm không tồn tại" });

                // Check Stock
                if (variant.StockQuantity < request.Quantity)
                {
                    return Json(new { success = false, message = $"Xin lỗi, chỉ còn {variant.StockQuantity} sản phẩm trong kho" });
                }

                var cart = GetCartFromSession();
                var existingItem = cart.FirstOrDefault(x => x.VariantId == request.VariantId);

                if (existingItem != null)
                {
                    if (variant.StockQuantity < (existingItem.Quantity + request.Quantity))
                    {
                        return Json(new { success = false, message = $"Xin lỗi, bạn đã có {existingItem.Quantity} sản phẩm trong giỏ. Chỉ còn {variant.StockQuantity} sản phẩm có sẵn" });
                    }
                    existingItem.Quantity += request.Quantity;
                }
                else
                {
                    // Get product image from ProductImages table
                    var productImage = await _context.ProductImages
                        .Where(pi => pi.ProductId == request.ProductId && pi.IsMain)
                        .Select(pi => pi.ImageUrl)
                        .FirstOrDefaultAsync();

                    cart.Add(new CartItemViewModel
                    {
                        ProductId = request.ProductId,
                        VariantId = request.VariantId,
                        ProductName = variant.Product.Name,
                        VariantName = variant.Sku,
                        Price = variant.Price,
                        Quantity = request.Quantity,
                        ImageUrl = productImage ?? $"https://placehold.co/100x100?text={variant.Product.Name}"
                    });
                }

                SaveCartToSession(cart);
                return Json(new { success = true, message = "Đã thêm vào giỏ hàng!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> AddToCart(int productId, int variantId, int quantity)
        {
            try
            {
                var variant = await _context.ProductVariants
                    .Include(v => v.Product)
                    .FirstOrDefaultAsync(v => v.Id == variantId);

                if (variant == null) return NotFound();

                // Check Stock
                if (variant.StockQuantity < quantity)
                {
                    TempData["Error"] = $"Xin lỗi, chỉ còn {variant.StockQuantity} sản phẩm trong kho.";
                    return RedirectToAction("Details", "Product", new { id = productId });
                }

                var cart = GetCartFromSession();
                var existingItem = cart.FirstOrDefault(x => x.VariantId == variantId);

                if (existingItem != null)
                {
                    if (variant.StockQuantity < (existingItem.Quantity + quantity))
                    {
                        TempData["Error"] = $"Xin lỗi, bạn đã có {existingItem.Quantity} sản phẩm trong giỏ. Chỉ còn {variant.StockQuantity} sản phẩm có sẵn.";
                        return RedirectToAction("Details", "Product", new { id = productId });
                    }
                    existingItem.Quantity += quantity;
                }
                else
                {
                    // Get product image from ProductImages table
                    var productImage = await _context.ProductImages
                        .Where(pi => pi.ProductId == productId && pi.IsMain)
                        .Select(pi => pi.ImageUrl)
                        .FirstOrDefaultAsync();

                    cart.Add(new CartItemViewModel
                    {
                        ProductId = productId,
                        VariantId = variantId,
                        ProductName = variant.Product.Name,
                        VariantName = variant.Sku,
                        Price = variant.Price,
                        Quantity = quantity,
                        ImageUrl = productImage ?? $"https://placehold.co/100x100?text={variant.Product.Name}"
                    });
                }

                SaveCartToSession(cart);
                TempData["Success"] = "Đã thêm vào giỏ hàng!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi thêm vào giỏ hàng: " + ex.Message;
                return RedirectToAction("Details", "Product", new { id = productId });
            }
        }

        [Authorize]
        public IActionResult Remove(int variantId)
        {
            var cart = GetCartFromSession();
            var item = cart.FirstOrDefault(x => x.VariantId == variantId);
            if (item != null)
            {
                cart.Remove(item);
                SaveCartToSession(cart);
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var cart = GetCartFromSession();
            if (!cart.Any()) return RedirectToAction(nameof(Index));

            var user = await _userManager.GetUserAsync(User);
            
            var model = new CheckoutViewModel
            {
                CartItems = cart,
                FullName = user.FullName,
                Address = user.Address,
                City = user.City,
                PhoneNumber = user.PhoneNumber,
                AvailablePoints = user.LoyaltyPoints
            };

            // Check for applied coupon
            var couponCode = HttpContext.Session.GetString(CouponSessionKey);
            if (!string.IsNullOrEmpty(couponCode))
            {
                var coupon = await _context.Coupons
                    .FirstOrDefaultAsync(c => c.Code == couponCode && c.IsActive && c.ExpiryDate > DateTime.Now);

                if (coupon != null)
                {
                    var total = cart.Sum(x => x.Total);
                    if (total >= coupon.MinOrderAmount)
                    {
                        model.CouponCode = coupon.Code;
                        model.DiscountAmount = CalculateCouponDiscount(coupon, total);
                    }
                    else
                    {
                        // Coupon no longer valid for this amount
                        HttpContext.Session.Remove(CouponSessionKey);
                    }
                }
                else
                {
                    // Coupon expired or invalid
                    HttpContext.Session.Remove(CouponSessionKey);
                }
            }

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            Console.WriteLine("=== Checkout POST started ===");
            Console.WriteLine($"FullName: {model.FullName}");
            Console.WriteLine($"PaymentMethod: {model.PaymentMethod}");
            Console.WriteLine($"Address: {model.Address}");
            Console.WriteLine($"City: {model.City}");
            Console.WriteLine($"PhoneNumber: {model.PhoneNumber}");
            
            var cart = GetCartFromSession();
            if (!cart.Any())
            {
                Console.WriteLine("Cart is empty, redirecting to Index");
                return RedirectToAction(nameof(Index));
            }

            // Re-validate coupon
            var couponCode = HttpContext.Session.GetString(CouponSessionKey);
            Coupon coupon = null;
            if (!string.IsNullOrEmpty(couponCode))
            {
                coupon = await _context.Coupons
                    .FirstOrDefaultAsync(c => c.Code == couponCode && c.IsActive && c.ExpiryDate > DateTime.Now);
                
                if (coupon != null)
                {
                     var total = cart.Sum(x => x.Total);
                     if (total >= coupon.MinOrderAmount)
                     {
                         model.CouponCode = coupon.Code;
                         model.DiscountAmount = CalculateCouponDiscount(coupon, total);
                     }
                     else
                     {
                         coupon = null; // Invalid
                     }
                }
            }

            // Remove card validation errors if not using Credit Card
            if (model.PaymentMethod != PaymentMethod.CreditCard)
            {
                ModelState.Remove("CardNumber");
                ModelState.Remove("CardHolderName");
                ModelState.Remove("ExpiryDate");
                ModelState.Remove("CVV");
            }

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is invalid:");
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"  {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
                model.CartItems = cart;
                return View(model);
            }

            // Mock Payment Validation
            if (model.PaymentMethod == PaymentMethod.CreditCard)
            {
                if (string.IsNullOrWhiteSpace(model.CardNumber) || model.CardNumber.Length < 16)
                {
                    ModelState.AddModelError("CardNumber", "Invalid Card Number");
                    model.CartItems = cart;
                    return View(model);
                }
                // Simulate processing delay
                await Task.Delay(1000);
            }

            var user = await _userManager.GetUserAsync(User);

            // Calculate Points Discount
            decimal pointsDiscount = 0;
            if (model.UseLoyaltyPoints && user.LoyaltyPoints > 0)
            {
                // 1 Point = 1000 VND (Example)
                pointsDiscount = user.LoyaltyPoints * 1000;
                
                // Cap discount at 50% of total
                decimal couponDiscount = coupon != null ? CalculateCouponDiscount(coupon, cart.Sum(x => x.Total)) : 0;
                var maxDiscount = (cart.Sum(x => x.Total) - couponDiscount) * 0.5m;
                if (pointsDiscount > maxDiscount)
                {
                    pointsDiscount = maxDiscount;
                }
            }

            // Create Order
            decimal finalCouponDiscount = coupon != null ? CalculateCouponDiscount(coupon, cart.Sum(x => x.Total)) : 0;
            var order = new Order
            {
                UserId = user.Id,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.PendingPayment,
                FullName = model.FullName,
                ShippingAddress = model.Address,
                ShippingCity = model.City,
                PhoneNumber = model.PhoneNumber,
                Note = model.Note ?? "",
                TotalAmount = cart.Sum(x => x.Total) - finalCouponDiscount - pointsDiscount,
                CouponCode = coupon?.Code ?? "",
                DiscountAmount = finalCouponDiscount + pointsDiscount,
                PaymentMethod = model.PaymentMethod,
                PaymentStatus = "Pending",
                TransactionId = ""
            };

            // Increment coupon usage
            if (coupon != null)
            {
                coupon.UsedCount++;
                _context.Update(coupon);
            }

            // Deduct Points
            if (pointsDiscount > 0)
            {
                // Calculate points used (reverse logic)
                int pointsUsed = (int)Math.Ceiling(pointsDiscount / 1000);
                user.LoyaltyPoints -= pointsUsed;
            }

            // Award New Points (1 point per 100,000 VND)
            int pointsEarned = (int)(order.TotalAmount / 100000);
            user.LoyaltyPoints += pointsEarned;
            
            await _userManager.UpdateAsync(user);
            
            // Create Order Items and Update Stock
            foreach (var item in cart)
            {
                var variant = await _context.ProductVariants
                    .Include(v => v.Product)
                    .FirstOrDefaultAsync(v => v.Id == item.VariantId);
                    
                if (variant == null || variant.StockQuantity < item.Quantity)
                {
                    TempData["Error"] = $"Product {item.ProductName} ({item.VariantName}) is out of stock or insufficient quantity.";
                    return RedirectToAction(nameof(Index));
                }

                // Decrement Stock
                variant.StockQuantity -= item.Quantity;
                
                // Check if product is sold out after this order
                var product = variant.Product;
                var totalStock = await _context.ProductVariants
                    .Where(v => v.ProductId == product.Id)
                    .SumAsync(v => v.StockQuantity);
                
                // Handle product sold behavior
                if (totalStock == 0)
                {
                    if (product.SoldBehavior == Domain.Enums.ProductSoldBehavior.AutoHide)
                    {
                        // Auto hide product when sold out
                        product.IsPublished = false;
                    }
                    // If ShowOutOfStock, product stays published but stock is 0
                    // The UI will handle showing "Out of Stock" badge
                }

                order.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductVariantId = item.VariantId,
                    ProductName = item.ProductName,
                    Sku = item.VariantName,
                    UnitPrice = item.Price,
                    Quantity = item.Quantity
                });
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Notify Admin
         //   await _hubContext.Clients.All.SendAsync("ReceiveNotification", $"New Order #{order.Id} placed by {user.FullName}!");

            // // Send confirmation email
            // try
            // {
            //     await _emailService.SendEmailAsync(user.Email, $"Order Confirmation #{order.Id}", $"Thank you for your order! Total: {order.TotalAmount:N0} đ");
            // }
            // catch (Exception ex)
            // {
            //     Console.WriteLine($"Email error: {ex.Message}");
            // }

            // Clear Cart and Coupon
            HttpContext.Session.Remove(CartSessionKey);
            HttpContext.Session.Remove(CouponSessionKey);

            // Handle different payment methods
            Console.WriteLine($"Payment method selected: {model.PaymentMethod}");
            
            if (model.PaymentMethod != PaymentMethod.COD)
            {
                // For online payments (CreditCard, BankTransfer), redirect to Mock VNPay payment gateway
                // This replaces the actual API call for demo purposes
                return RedirectToAction("PaymentMock", new { orderId = order.Id, amount = order.TotalAmount });
            }
            else
            {
                // For COD (Cash on Delivery), redirect to order confirmation page
                Console.WriteLine($"COD payment selected, redirecting to OrderConfirmation");
                return RedirectToAction("OrderConfirmation", new { id = order.Id });
            }
        }

        [Authorize]
        [HttpGet]
        public IActionResult PaymentMock(int orderId, decimal amount)
        {
            ViewBag.Amount = amount;
            ViewBag.OrderId = orderId;
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PaymentMockConfirm(int orderId, bool success)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound();

            if (success)
            {
                // Payment successful
                order.Status = OrderStatus.Paid;
                
                // You might want to trigger email/notification here
                TempData["Success"] = "Thanh toán thành công!";
            }
            else
            {
                // Payment cancelled or failed
                order.Status = OrderStatus.Cancelled;
                TempData["Error"] = "Thanh toán đã bị hủy hoặc thất bại.";
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction("OrderConfirmation", new { id = orderId });
        }

        private decimal CalculateCouponDiscount(Coupon coupon, decimal orderTotal)
        {
            switch (coupon.Type)
            {
                case CouponType.FixedAmount:
                    return coupon.DiscountValue;
                
                case CouponType.Percentage:
                    var percentDiscount = orderTotal * (coupon.DiscountValue / 100);
                    if (coupon.MaxDiscountAmount.HasValue && percentDiscount > coupon.MaxDiscountAmount.Value)
                    {
                        return coupon.MaxDiscountAmount.Value;
                    }
                    return percentDiscount;
                
                case CouponType.FreeShipping:
                    // Free shipping doesn't reduce order total, handled separately
                    return 0;
                
                default:
                    return 0;
            }
        }

        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();
            
            return View(order);
        }

        [Authorize]
        public async Task<IActionResult> PaymentCallback()
        {
            var response = _vnpayClient.GetPaymentResponse(Request.Query);
            
            if (response.Success)
            {
                // Extract order ID from orderInfo
                var orderInfo = response.OrderInfo;
                if (orderInfo.StartsWith("order_"))
                {
                    var orderIdStr = orderInfo.Replace("order_", "");
                    if (int.TryParse(orderIdStr, out int orderId))
                    {
                        var order = await _context.Orders.FindAsync(orderId);
                        if (order != null)
                        {
                            order.Status = OrderStatus.Paid;
                            order.PaymentDate = DateTime.Now;
                            order.TransactionId = response.TransactionId;
                            await _context.SaveChangesAsync();
                            
                            return RedirectToAction("PaymentSuccess", new { orderId = orderId });
                        }
                    }
                }
            }
            
            return RedirectToAction("PaymentFailed", new { reason = response.Message });
        }

        [Authorize]
        public async Task<IActionResult> PaymentSuccess(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return NotFound();

            TempData["Success"] = $"Thanh toán thành công! Đơn hàng #{order.Id} đã được xác nhận.";
            return View("OrderConfirmation", order);
        }

        [Authorize]
        public async Task<IActionResult> PaymentFailed(string reason)
        {
            TempData["Error"] = $"Thanh toán thất bại: {reason}. Vui lòng thử lại hoặc chọn phương thức thanh toán khác.";
            return RedirectToAction("Index");
        }

        private List<CartItemViewModel> GetCartFromSession()
        {
            var sessionData = HttpContext.Session.GetString(CartSessionKey);
            return sessionData == null 
                ? new List<CartItemViewModel>() 
                : JsonSerializer.Deserialize<List<CartItemViewModel>>(sessionData);
        }

        private void SaveCartToSession(List<CartItemViewModel> cart)
        {
            HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
        }
    }
}
