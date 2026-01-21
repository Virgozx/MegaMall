using MegaMall.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MegaMall.Controllers
{
    public class SearchController : Controller
    {
        private readonly MallDbContext _context;

        public SearchController(MallDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Suggestions(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 2)
            {
                return Json(new List<object>());
            }

            var suggestions = new List<object>();

            // Search in Products
            var productSuggestions = await _context.Products
                .Where(p => p.IsPublished && !p.IsDeleted && p.Name.Contains(keyword))
                .Select(p => new
                {
                    keyword = p.Name,
                    category = p.Category != null ? p.Category.Name : "Sản phẩm"
                })
                .Take(7)
                .ToListAsync();

            suggestions.AddRange(productSuggestions);

            // Search in Categories
            var categorySuggestions = await _context.Categories
                .Where(c => c.Name.Contains(keyword))
                .Select(c => new
                {
                    keyword = c.Name,
                    category = "Danh mục"
                })
                .Take(3)
                .ToListAsync();

            suggestions.AddRange(categorySuggestions);

            return Json(suggestions.Take(10));
        }

        [HttpGet]
        public async Task<IActionResult> Index(string q, int? categoryId, string location, string sortBy = "popular", int page = 1)
        {
            var query = _context.Products
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .Include(p => p.Category)
                .Where(p => p.IsPublished && !p.IsDeleted);

            // Filter by search query
            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(p => p.Name.Contains(q) || p.Description.Contains(q));
            }

            // Filter by category
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // Sorting
            query = sortBy switch
            {
                "newest" => query.OrderByDescending(p => p.CreatedDate),
                "price_asc" => query.OrderBy(p => p.Variants.Min(v => v.Price)),
                "price_desc" => query.OrderByDescending(p => p.Variants.Max(v => v.Price)),
                _ => query.OrderByDescending(p => p.CreatedDate) // popular default
            };

            // Pagination
            int pageSize = 20;
            int totalProducts = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.SearchQuery = q;
            ViewBag.CategoryId = categoryId;
            ViewBag.Location = location;
            ViewBag.SortBy = sortBy;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Categories = await _context.Categories.ToListAsync();

            return View("~/Views/Home/Index.cshtml", products);
        }
    }
}
