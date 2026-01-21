using MegaMall.Domain.Entities;
using MegaMall.ViewModels;
using MegaMall.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace MegaMall.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailService _emailService;

        // In-memory OTP storage (for demo - use Redis/Database in production)
        private static Dictionary<string, (string Otp, DateTime Expiry)> _otpStore = new();

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser 
                { 
                    UserName = model.Email, 
                    Email = model.Email,
                    FullName = model.FullName,
                    Address = "", // Set default empty string instead of null
                    City = "", // Set default empty string instead of null
                    ShopName = model.IsSeller ? model.ShopName : null,
                    ShopDescription = "", // Set default empty string
                    IsSellerApproved = false // Sellers need approval
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Assign Role
                    string roleName = model.IsSeller ? "Seller" : "Buyer";
                    
                    // Ensure roles exist
                    if (!await _roleManager.RoleExistsAsync(roleName))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                    
                    await _userManager.AddToRoleAsync(user, roleName);

                    // Send Welcome Email
                    try 
                    {
                        string subject = "Welcome to MegaMall!";
                        string body = $"<h3>Hi {user.FullName},</h3><p>Thank you for registering at MegaMall.</p><p>Your account has been created successfully.</p>";
                        await _emailService.SendEmailAsync(user.Email, subject, body);
                    }
                    catch
                    {
                        // Log error but don't stop registration
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return RedirectToLocal(returnUrl);
                }
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Email không hợp lệ" });
            }

            // Generate 6-digit OTP
            var random = new Random();
            var otp = random.Next(100000, 999999).ToString();

            // Store OTP with 5 minutes expiry
            _otpStore[model.Email.ToLower()] = (otp, DateTime.UtcNow.AddMinutes(5));

            // Send OTP via email
            var sent = await _emailService.SendOTP(otp, model.Email);

            if (sent)
            {
                return Json(new { success = true, message = "Mã OTP đã được gửi đến email của bạn" });
            }
            else
            {
                return Json(new { success = false, message = "Không thể gửi email. Vui lòng thử lại" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Thông tin không hợp lệ" });
            }

            var email = model.Email.ToLower();

            // Check if OTP exists
            if (!_otpStore.ContainsKey(email))
            {
                return Json(new { success = false, message = "Mã OTP không tồn tại hoặc đã hết hạn" });
            }

            var (storedOtp, expiry) = _otpStore[email];

            // Check if OTP expired
            if (DateTime.UtcNow > expiry)
            {
                _otpStore.Remove(email);
                return Json(new { success = false, message = "Mã OTP đã hết hạn" });
            }

            // Verify OTP
            if (storedOtp != model.Otp)
            {
                return Json(new { success = false, message = "Mã OTP không đúng" });
            }

            // OTP is valid - remove it
            _otpStore.Remove(email);

            // Check if user exists
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                // Create new user
                user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.Email.Split('@')[0],
                    Address = "",
                    City = "",
                    ShopDescription = "",
                    EmailConfirmed = true // Email verified via OTP
                };

                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    return Json(new { success = false, message = "Không thể tạo tài khoản" });
                }

                // Assign Buyer role
                if (!await _roleManager.RoleExistsAsync("Buyer"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Buyer"));
                }
                await _userManager.AddToRoleAsync(user, "Buyer");
            }

            // Sign in the user
            await _signInManager.SignInAsync(user, isPersistent: true);

            return Json(new { success = true, message = "Đăng nhập thành công" });
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
