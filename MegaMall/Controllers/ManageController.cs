using MegaMall.Domain.Entities;
using MegaMall.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MegaMall.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly MallDbContext _context;

        public ManageController(UserManager<ApplicationUser> userManager, MallDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(ApplicationUser model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.FullName = model.FullName ?? user.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address ?? "";
            user.City = model.City ?? "";
            user.AvatarUrl = model.AvatarUrl ?? user.AvatarUrl;

            await _userManager.UpdateAsync(user);
            TempData["StatusMessage"] = "Profile updated successfully";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Addresses()
        {
            var user = await _userManager.GetUserAsync(User);
            var addresses = await _context.UserAddresses
                .Where(a => a.UserId == user.Id)
                .ToListAsync();
            return View(addresses);
        }

        public IActionResult AddAddress()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddAddress(UserAddress address)
        {
            var user = await _userManager.GetUserAsync(User);
            address.UserId = user.Id;

            if (address.IsDefault)
            {
                var defaults = await _context.UserAddresses
                    .Where(a => a.UserId == user.Id && a.IsDefault)
                    .ToListAsync();
                foreach (var d in defaults) d.IsDefault = false;
            }

            _context.UserAddresses.Add(address);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Addresses));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var address = await _context.UserAddresses
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);
            
            if (address != null)
            {
                _context.UserAddresses.Remove(address);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Addresses));
        }
    }
}
