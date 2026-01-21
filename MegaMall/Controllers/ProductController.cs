using MegaMall.Data;
using MegaMall.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MegaMall.Controllers
{
    public class ProductController : Controller
    {
        private readonly MallDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductController(MallDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Variants)
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            // Handle Recently Viewed (Cookie)
            var recentlyViewedIds = new List<int>();
            var cookieKey = "RecentlyViewed";
            if (Request.Cookies.TryGetValue(cookieKey, out string cookieValue) && !string.IsNullOrEmpty(cookieValue))
            {
                // Robust parsing to avoid crashes if cookie is tampered
                foreach (var item in cookieValue.Split(','))
                {
                    if (int.TryParse(item, out int parsedId))
                    {
                        recentlyViewedIds.Add(parsedId);
                    }
                }
            }

            // Remove if exists to move to top
            recentlyViewedIds.Remove(id);
            recentlyViewedIds.Insert(0, id);
            // Keep only last 5
            if (recentlyViewedIds.Count > 5) recentlyViewedIds = recentlyViewedIds.Take(5).ToList();

            // Save back to cookie
            var options = new CookieOptions { Expires = DateTime.Now.AddDays(30) };
            Response.Cookies.Append(cookieKey, string.Join(",", recentlyViewedIds), options);

            // Get Recently Viewed Products Data (excluding current)
            var recentIdsToFetch = recentlyViewedIds.Where(x => x != id).ToList();
            var recentlyViewedProducts = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .Where(p => recentIdsToFetch.Contains(p.Id))
                .ToListAsync();
            
            // Order them by the order in the cookie list
            recentlyViewedProducts = recentlyViewedProducts
                .OrderBy(p => recentIdsToFetch.IndexOf(p.Id))
                .ToList();

            ViewBag.RecentlyViewedProducts = recentlyViewedProducts;

            // Get Related Products - Improved Algorithm
            // Strategy: Prioritize products that are most similar to current product
            var currentPrice = product.Variants.Any() ? product.Variants.Average(v => v.Price) : 0;
            var priceMin = currentPrice * 0.7m; // 30% lower
            var priceMax = currentPrice * 1.5m; // 50% higher
            
            // Extract keywords from product name for better matching
            var productKeywords = product.Name.ToLower()
                .Split(new[] { ' ', ',', '-', '/' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 3) // Only meaningful words
                .ToList();

            var relatedProductsQuery = _context.Products
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .Include(p => p.Reviews)
                .Where(p => p.Id != product.Id && p.IsPublished && !p.IsDeleted);

            // Priority 1: Same category + similar price range + keyword match
            // Optimization: Filter in DB first to avoid loading entire category into memory
            // Taking top 100 candidates based on creation date (newest) or meaningful usage
            var sameCategoryProducts = await relatedProductsQuery
                .Where(p => p.CategoryId == product.CategoryId)
                .OrderByDescending(p => p.CreatedDate)
                .Take(100) 
                .ToListAsync();

            var scoredProducts = sameCategoryProducts
                .Select(p => new
                {
                    Product = p,
                    Score = CalculateRelevanceScore(p, product, productKeywords, currentPrice, priceMin, priceMax)
                })
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Product.Reviews.Any() ? x.Product.Reviews.Average(r => r.Rating) : 0) // Higher rated first
                .ThenByDescending(x => x.Product.CreatedDate) // Newer products
                .Take(4)
                .Select(x => x.Product)
                .ToList();

            // If not enough products in same category, get from other categories with keyword match
            if (scoredProducts.Count < 4)
            {
                var otherProducts = await relatedProductsQuery
                    .Where(p => p.CategoryId != product.CategoryId)
                    .ToListAsync();

                var additionalProducts = otherProducts
                    .Select(p => new
                    {
                        Product = p,
                        Score = CalculateRelevanceScore(p, product, productKeywords, currentPrice, priceMin, priceMax)
                    })
                    .Where(x => x.Score > 0) // Only products with some relevance
                    .OrderByDescending(x => x.Score)
                    .Take(4 - scoredProducts.Count)
                    .Select(x => x.Product)
                    .ToList();

                scoredProducts.AddRange(additionalProducts);
            }

            ViewBag.RelatedProducts = scoredProducts;

            // AI Recommendations (Mock - Frequently Bought Together)
            // Logic: Find orders that contain this product, then find other products in those orders
            var orderIds = await _context.OrderItems
                .Where(oi => oi.ProductId == id)
                .Select(oi => oi.OrderId)
                .Distinct()
                .Take(50) // Limit to recent 50 orders for performance
                .ToListAsync();

            var frequentlyBoughtIds = await _context.OrderItems
                .Where(oi => orderIds.Contains(oi.OrderId) && oi.ProductId != id)
                .GroupBy(oi => oi.ProductId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(4)
                .ToListAsync();

            var frequentlyBoughtProducts = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .Where(p => frequentlyBoughtIds.Contains(p.Id))
                .ToListAsync();

            // Fallback if not enough data: Random products
            if (frequentlyBoughtProducts.Count < 4)
            {
                var randomProducts = await _context.Products
                    .Include(p => p.Images)
                    .Include(p => p.Variants)
                    .Where(p => p.Id != id && !frequentlyBoughtIds.Contains(p.Id))
                    .OrderBy(r => Guid.NewGuid())
                    .Take(4 - frequentlyBoughtProducts.Count)
                    .ToListAsync();
                
                frequentlyBoughtProducts.AddRange(randomProducts);
            }

            ViewBag.FrequentlyBoughtTogether = frequentlyBoughtProducts;

            // Check if product is in wishlist
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var isInWishlist = await _context.WishlistItems
                        .AnyAsync(w => w.UserId == user.Id && w.ProductId == id);
                    ViewBag.IsInWishlist = isInWishlist;
                    
                    // Check if user has already reviewed this product
                    var userReview = await _context.Reviews
                        .FirstOrDefaultAsync(r => r.UserId == user.Id && r.ProductId == id);
                    ViewBag.UserReview = userReview;
                }
            }

            ViewData["MetaDescription"] = product.Description.Length > 150 ? product.Description.Substring(0, 147) + "..." : product.Description;
            ViewData["MetaKeywords"] = $"{product.Name}, buy {product.Name}, {product.Name} price";

            return View(product);
        }

        /// <summary>
        /// Calculate relevance score between two products
        /// Higher score = more relevant
        /// </summary>
        private int CalculateRelevanceScore(Product candidate, Product current, List<string> currentKeywords, decimal currentPrice, decimal priceMin, decimal priceMax)
        {
            int score = 0;
            
            var candidateName = candidate.Name.ToLower();
            var candidateDesc = candidate.Description?.ToLower() ?? "";
            
            // 1. Same category = +50 points
            if (candidate.CategoryId == current.CategoryId)
            {
                score += 50;
            }
            
            // 2. Same seller = +30 points (may want to see other products from same seller)
            if (candidate.SellerId == current.SellerId)
            {
                score += 30;
            }
            
            // 3. Price similarity = +40 points (in same price range)
            if (candidate.Variants.Any())
            {
                var candidatePrice = candidate.Variants.Average(v => v.Price);
                if (candidatePrice >= priceMin && candidatePrice <= priceMax)
                {
                    score += 40;
                    
                    // Bonus: very close price (+10 more)
                    var priceDiff = Math.Abs(candidatePrice - currentPrice);
                    if (priceDiff < currentPrice * 0.2m) // Within 20%
                    {
                        score += 10;
                    }
                }
            }
            
            // 4. Keyword matching in name = +10 points per keyword (max 50)
            var keywordMatches = currentKeywords.Count(keyword => candidateName.Contains(keyword));
            score += Math.Min(keywordMatches * 10, 50);
            
            // 5. Keyword matching in description = +5 points per keyword (max 25)
            var descKeywordMatches = currentKeywords.Count(keyword => candidateDesc.Contains(keyword));
            score += Math.Min(descKeywordMatches * 5, 25);
            
            // 6. Good ratings = +15 points
            if (candidate.Reviews.Any())
            {
                var avgRating = candidate.Reviews.Average(r => r.Rating);
                if (avgRating >= 4.0)
                {
                    score += 15;
                }
                else if (avgRating >= 3.5)
                {
                    score += 8;
                }
            }
            
            // 7. New product = +10 points (within last 30 days)
            if (candidate.CreatedDate >= DateTime.Now.AddDays(-30))
            {
                score += 10;
            }
            
            return score;
        }    }
}