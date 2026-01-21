using MegaMall.Domain.Entities;
using MegaMall.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using MegaMall.Models;
using Microsoft.Extensions.Caching.Memory;
using MegaMall.Interfaces;

namespace MegaMall.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MallDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ICalendarService _calendarService;

        public HomeController(ILogger<HomeController> logger, MallDbContext context, IMemoryCache cache, ICalendarService calendarService)
        {
            _logger = logger;
            _context = context;
            _cache = cache;
            _calendarService = calendarService;
        }

        public async Task<IActionResult> Index(string query, int? categoryId, string category, decimal? minPrice, decimal? maxPrice, string sortBy, int page = 1)
        {
            // Cache categories as they don't change often
            if (!_cache.TryGetValue("Categories", out List<Category> categories))
            {
                categories = await _context.Categories.ToListAsync();
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30));
                _cache.Set("Categories", categories, cacheEntryOptions);
            }
            ViewBag.Categories = categories;

            // Get active vouchers for display
            var activeCoupons = await _context.Coupons
                .Where(c => c.IsActive 
                    && c.ExpiryDate > DateTime.Now 
                    //&& c.StartDate <= DateTime.Now // Allow upcoming coupons to show as "Coming Soon"
                    && c.UsedCount < (c.Quantity ?? int.MaxValue))
                .OrderByDescending(c => c.CreatedDate) // Prioritize new coupons
                .Take(6)
                .ToListAsync();
            ViewBag.ActiveCoupons = activeCoupons;
            
            // Get upcoming holidays for notification
            var upcomingHolidays = await _calendarService.GetUpcomingVietnameseHolidaysAsync(1);
            var nextHoliday = upcomingHolidays.FirstOrDefault();
            ViewBag.NextHoliday = nextHoliday;

            int pageSize = 10;
            var productsQuery = _context.Products
                .Include(p => p.Variants)
                .Include(p => p.Images)
                .Include(p => p.Category)
                .Where(p => p.IsPublished && !p.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrEmpty(query))
            {
                productsQuery = productsQuery.Where(p => p.Name.Contains(query) || p.Description.Contains(query));
            }

            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
            }

            // Filter by category name (from icon click)
            if (!string.IsNullOrEmpty(category))
            {
                productsQuery = productsQuery.Where(p => p.Category.Name == category);
            }

            // Filter by price (using the lowest price variant)
            if (minPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Variants.Any(v => v.Price >= minPrice.Value));
            }

            if (maxPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Variants.Any(v => v.Price <= maxPrice.Value));
            }

            // Sorting
            switch (sortBy)
            {
                case "price_asc":
                    productsQuery = productsQuery.OrderBy(p => p.Variants.Min(v => v.Price));
                    break;
                case "price_desc":
                    productsQuery = productsQuery.OrderByDescending(p => p.Variants.Min(v => v.Price));
                    break;
                default: // Newest
                    productsQuery = productsQuery.OrderByDescending(p => p.CreatedDate);
                    break;
            }

            var totalItems = await productsQuery.CountAsync();
            var products = await productsQuery.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.CurrentQuery = query;
            ViewBag.CurrentCategory = categoryId;
            ViewBag.SelectedCategory = category;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.SortBy = sortBy;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            ViewData["MetaDescription"] = "Browse our vast collection of products at MegaMall. Best prices and fast shipping.";
            ViewData["MetaKeywords"] = "megamall, shop, electronics, fashion, home";

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ProductList", products);
            }

            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
