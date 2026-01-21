using MegaMall.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace MegaMall.Controllers
{
    public class CompareController : Controller
    {
        private readonly MallDbContext _context;
        private const string CompareSessionKey = "CompareSession";

        public CompareController(MallDbContext context)
        {
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var compareIds = GetCompareIds();
            if (!compareIds.Any())
            {
                return View(new List<MegaMall.Domain.Entities.Product>());
            }

            var products = await _context.Products
                .Include(p => p.Variants)
                .Include(p => p.Images)
                .Where(p => compareIds.Contains(p.Id))
                .ToListAsync();

            return View(products);
        }

        [Authorize]
        [HttpPost]
        public IActionResult AddToCompare(int id)
        {
            var compareIds = GetCompareIds();
            if (!compareIds.Contains(id))
            {
                if (compareIds.Count >= 3)
                {
                    TempData["Error"] = "You can only compare up to 3 products.";
                }
                else
                {
                    compareIds.Add(id);
                    SaveCompareIds(compareIds);
                    TempData["Success"] = "Product added to comparison.";
                }
            }
            else
            {
                TempData["Info"] = "Product is already in comparison list.";
            }
            
            // Return to previous page
            return Redirect(Request.Headers["Referer"].ToString());
        }

        [Authorize]
        public IActionResult Remove(int id)
        {
            var compareIds = GetCompareIds();
            if (compareIds.Contains(id))
            {
                compareIds.Remove(id);
                SaveCompareIds(compareIds);
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public IActionResult Clear()
        {
            HttpContext.Session.Remove(CompareSessionKey);
            return RedirectToAction(nameof(Index));
        }

        private List<int> GetCompareIds()
        {
            var sessionData = HttpContext.Session.GetString(CompareSessionKey);
            return sessionData == null 
                ? new List<int>() 
                : JsonSerializer.Deserialize<List<int>>(sessionData);
        }

        private void SaveCompareIds(List<int> ids)
        {
            HttpContext.Session.SetString(CompareSessionKey, JsonSerializer.Serialize(ids));
        }
    }
}
