using MegaMall.Domain.Entities;
using MegaMall.Data;
using MegaMall.Areas.Seller.ViewModels;
using MegaMall.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace MegaMall.Areas.Seller.Controllers
{
    [Area("Seller")]
    [Authorize(Roles = "Seller")]
    public class ProductController : Controller
    {
        private readonly MallDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileUploadService _fileUploadService;

        public ProductController(MallDbContext context, UserManager<ApplicationUser> userManager, IFileUploadService fileUploadService)
        {
            _context = context;
            _userManager = userManager;
            _fileUploadService = fileUploadService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var products = await _context.Products
                .Include(p => p.Variants)
                .Where(p => p.SellerId == user.Id && !p.IsDeleted)
                .OrderByDescending(p => p.Id)
                .ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || !user.IsSellerApproved)
            {
                return RedirectToAction("Index", "Dashboard"); // Or show an error page
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || !user.IsSellerApproved)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                return View(model);
            }

            // Validate variants
            if (model.Variants == null || !model.Variants.Any())
            {
                ModelState.AddModelError("", "Vui lòng thêm ít nhất một biến thể sản phẩm");
                ViewBag.Categories = await _context.Categories.ToListAsync();
                return View(model);
            }

            // Create Product
            var product = new Product
            {
                Name = model.Name,
                Description = model.Description,
                SellerId = user.Id,
                CategoryId = model.CategoryId,
                IsPublished = true,
                IsDeleted = false,
                SoldBehavior = model.SoldBehavior,
                AttributesJson = JsonSerializer.Serialize(new { Material = model.Material ?? "N/A" })
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Handle Image Upload using FileUploadService
            if (model.Images != null && model.Images.Any())
            {
                var imagePaths = await _fileUploadService.UploadFilesAsync(model.Images, "products/images");
                
                for (int i = 0; i < imagePaths.Count; i++)
                {
                    var productImage = new ProductImage
                    {
                        ProductId = product.Id,
                        ImageUrl = imagePaths[i],
                        IsMain = i == 0 // Đặt ảnh đầu tiên làm ảnh chính
                    };
                    _context.ProductImages.Add(productImage);
                }
                await _context.SaveChangesAsync();
            }

            // Handle Video Upload using FileUploadService
            if (model.Videos != null && model.Videos.Any())
            {
                var videoPaths = await _fileUploadService.UploadFilesAsync(model.Videos, "products/videos");
                
                for (int i = 0; i < videoPaths.Count; i++)
                {
                    var productVideo = new ProductVideo
                    {
                        ProductId = product.Id,
                        VideoUrl = videoPaths[i],
                        ThumbnailUrl = "https://cdn-icons-png.flaticon.com/512/4404/4404094.png", // Default placeholder
                        DisplayOrder = i
                    };
                    _context.ProductVideos.Add(productVideo);
                }
                await _context.SaveChangesAsync();
            }

            // Create Variants
            foreach (var variantModel in model.Variants)
            {
                var variantProperties = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(variantModel.Color))
                    variantProperties["Màu sắc"] = variantModel.Color;
                if (!string.IsNullOrEmpty(variantModel.Size))
                    variantProperties["Kích thước"] = variantModel.Size;
                if (!string.IsNullOrEmpty(variantModel.OtherAttribute))
                    variantProperties["Khác"] = variantModel.OtherAttribute;

                var variant = new ProductVariant
                {
                    ProductId = product.Id,
                    Sku = variantModel.Sku,
                    Price = variantModel.Price,
                    OriginalPrice = variantModel.OriginalPrice ?? variantModel.Price * 1.2m,
                    StockQuantity = variantModel.StockQuantity,
                    VariantPropertiesJson = JsonSerializer.Serialize(variantProperties)
                };

                _context.ProductVariants.Add(variant);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Seller/Product/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Include(p => p.Variants)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(m => m.Id == id && m.SellerId == user.Id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Seller/Product/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var product = await _context.Products
                .Include(p => p.Variants)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id && p.SellerId == user.Id);

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(product);
        }

        // POST: Seller/Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile[] NewImages)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var existingProduct = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id && p.SellerId == user.Id);

            if (existingProduct == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingProduct.Name = product.Name;
                    existingProduct.Description = product.Description;
                    existingProduct.CategoryId = product.CategoryId;
                    existingProduct.IsPublished = product.IsPublished;

                    // Handle new images
                    if (NewImages != null && NewImages.Any())
                    {
                        var imagePaths = await _fileUploadService.UploadFilesAsync(NewImages.ToList(), "products/images");
                        
                        foreach (var imagePath in imagePaths)
                        {
                            var productImage = new ProductImage
                            {
                                ProductId = product.Id,
                                ImageUrl = imagePath,
                                IsMain = !existingProduct.Images.Any()
                            };
                            _context.ProductImages.Add(productImage);
                        }
                    }

                    _context.Update(existingProduct);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(product);
        }

        // GET: Seller/Product/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(m => m.Id == id && m.SellerId == user.Id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Seller/Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var product = await _context.Products
                .Include(p => p.Variants)
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.Id == id && p.SellerId == user.Id);

            if (product != null)
            {
                // Hard delete - Remove from database completely
                // Remove related records first to avoid foreign key issues
                
                // Remove wishlist items that reference this product
                var wishlistItems = await _context.WishlistItems
                    .Where(w => w.ProductId == id)
                    .ToListAsync();
                if (wishlistItems.Any())
                {
                    _context.WishlistItems.RemoveRange(wishlistItems);
                }

                if (product.Reviews != null && product.Reviews.Any())
                {
                    _context.Reviews.RemoveRange(product.Reviews);
                }

                if (product.Variants != null && product.Variants.Any())
                {
                    _context.ProductVariants.RemoveRange(product.Variants);
                }

                if (product.Images != null && product.Images.Any())
                {
                    _context.ProductImages.RemoveRange(product.Images);
                }

                // Finally remove the product itself
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
