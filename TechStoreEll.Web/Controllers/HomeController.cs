using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Infrastructure.Data;
using TechStoreEll.Core.Models;

namespace TechStoreEll.Web.Controllers;

public class HomeController(AppDbContext context) : Controller
{
    public async Task<IActionResult> Product(int id)
    {
        var variant = await context.ProductVariants
            .Include(v => v.Product)
            .ThenInclude(p => p.Category)
            .Include(v => v.Product)
            .ThenInclude(p => p.Reviews)
            .ThenInclude(r => r.User)
            .Include(v => v.ProductImages)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (variant == null)
            return NotFound();
        
        var approvedReviews = variant.Product.Reviews
            .Where(r => r.IsModerated)
            .ToList();
        
        var reviewsCount = approvedReviews.Count;
        var avgRating = reviewsCount > 0 ? (decimal?)approvedReviews.Average(r => r.Rating) : null;

        var vm = new ProductDetailViewModel
        {
            Id = variant.Id,
            Sku = variant.VariantCode,
            Name = variant.Product.Name!,
            CategoryName = variant.Product.Category?.Name ?? "Без категории",
            Description = variant.Product.Description,
            Price = variant.Price,
            AvgRating = avgRating,   
            ReviewsCount = reviewsCount,
            Color = variant.Color,
            StorageGb = variant.StorageGb,
            Ram = variant.Ram,
            Images = variant.ProductImages
                .OrderBy(i => i.SortOrder)
                .Select(i => new ProductImageViewModel
                {
                    ImageUrl = i.ImageUrl,
                    AltText = i.AltText,
                    SortOrder = i.SortOrder
                })
                .ToList(),
            Reviews = approvedReviews
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ProductReviewViewModel
                {
                    AuthorName = r.User != null 
                        ? $"{r.User.FirstName} {r.User.LastName.FirstOrDefault()}."
                        : "Покупатель",
                    CreatedAt = r.CreatedAt,
                    Rating = r.Rating,
                    Comment = r.Comment
                })
                .ToList()
        };

        return View(vm);
    }
    
    public async Task<IActionResult> Index(
    string? q = null, 
    string sort = "date-desc", 
    int? take = null, 
    int skip = 0,
    int? categoryId = null,
    int? minRam = null,
    int? maxRam = null,
    int? minStorage = null,
    int? maxStorage = null,
    decimal? minPrice = null,
    decimal? maxPrice = null,
    bool? inStock = null)
{
    int itemsPerPage;
    try
    {
        var userId = GetCurrentUserId();
        var userSettings = await context.UserSettings.FirstOrDefaultAsync(us => us.Id == userId); 
        itemsPerPage = userSettings?.ItemsPerPage ?? 20;
        
    }
    catch
    {
        itemsPerPage = 100;  
    }

    var finalTake = itemsPerPage;
    
    var query = context.ProductVariants
        .Where(pv => pv.Product.Active);

    if (!string.IsNullOrWhiteSpace(q))
    {
        var cleanQuery = q.Trim().ToLowerInvariant();
        query = query.Where(pv => 
            pv.Product.SearchVector!.Matches(EF.Functions.PlainToTsQuery("russian", cleanQuery))
        );
    }

    if (categoryId is > 0) 
        query = query.Where(pv => pv.Product.CategoryId == categoryId);
    
    if (minRam.HasValue)
        query = query.Where(pv => pv.Ram >= minRam.Value);
    
    if (maxRam.HasValue)
        query = query.Where(pv => pv.Ram <= maxRam.Value);

    if (minStorage.HasValue)
        query = query.Where(pv => pv.StorageGb >= minStorage.Value);
    
    if (maxStorage.HasValue)
        query = query.Where(pv => pv.StorageGb <= maxStorage.Value);

    if (minPrice.HasValue)
        query = query.Where(pv => pv.Price >= minPrice.Value);
    
    if (maxPrice.HasValue)
        query = query.Where(pv => pv.Price <= maxPrice.Value);
    
    if (inStock.HasValue && inStock.Value)
        query = query.Where(pv => pv.Inventories.Any(i => i.Quantity - i.Reserve > 0));

    var orderedQuery = sort switch
    {
        "price-asc" => query.OrderBy(pv => pv.Price),
        "price-desc" => query.OrderByDescending(pv => pv.Price),
        "date-asc" => query.OrderBy(pv => pv.Product.CreatedAt),
        "date-desc" => query.OrderByDescending(pv => pv.Product.CreatedAt),
        _ => query.OrderByDescending(pv => pv.Product.CreatedAt)
    };

    var variants = await orderedQuery
        .Include(pv => pv.Product)
            .ThenInclude(p => p.Category)
        .Include(pv => pv.Product)
            .ThenInclude(p => p.Reviews)
        .Include(pv => pv.ProductImages.Where(pi => pi.IsPrimary))
        .Include(pv => pv.Inventories)
        .Skip(skip)
        .Take(finalTake)
        .ToListAsync();

    var viewModels = variants.Select(pv =>
    {
        var approvedReviews = pv.Product.Reviews.Where(r => r.IsModerated).ToList();
        var reviewsCount = approvedReviews.Count;
        var avgRating = reviewsCount > 0 ? (decimal?)approvedReviews.Average(r => r.Rating) : null;

        var totalAvailable = pv.Inventories?.Sum(i => i.Quantity - i.Reserve) ?? 0;
        var isInStock = totalAvailable > 0;

        return new ProductViewModel
        {
            Id = pv.Id,
            Sku = pv.VariantCode,
            Name = pv.Product.Name!,
            CategoryName = pv.Product.Category?.Name ?? "Без категории",
            CategoryId = pv.Product.CategoryId,
            Price = pv.Price,
            Ram = pv.Ram,
            StorageSize = pv.StorageGb,
            PrimaryImageUrl = pv.ProductImages
                .Where(pi => pi.IsPrimary)
                .OrderBy(pi => pi.SortOrder)
                .Select(pi => pi.ImageUrl)
                .FirstOrDefault() ?? "",
            PrimaryImageAltText = pv.ProductImages
                .Where(pi => pi.IsPrimary)
                .OrderBy(pi => pi.SortOrder)
                .Select(pi => pi.AltText)
                .FirstOrDefault() ?? "",
            AvgRating = avgRating,
            ReviewsCount = reviewsCount,
            CreatedAt = pv.Product.CreatedAt,
            IsInStock = isInStock,
            StockQuantity = totalAvailable
        };
    }).ToList();

    var categories = await context.Categories.OrderBy(c => c.Name).ToListAsync();

    var priceRange = await context.ProductVariants
        .Where(pv => pv.Product.Active)
        .Select(pv => pv.Price)
        .ToListAsync();

    var minPriceOverall = priceRange.Count != 0 ? priceRange.Min() : 0;
    var maxPriceOverall = priceRange.Count != 0 ? priceRange.Max() : 100000;

    ViewData["CurrentSearch"] = q;
    ViewData["CurrentSort"] = sort;
    ViewData["Categories"] = categories;
    ViewData["SelectedCategoryId"] = categoryId;
    ViewData["MinRam"] = minRam;
    ViewData["MaxRam"] = maxRam;
    ViewData["MinStorage"] = minStorage;
    ViewData["MaxStorage"] = maxStorage;
    ViewData["MinPrice"] = minPrice;
    ViewData["MaxPrice"] = maxPrice;
    ViewData["MinPriceOverall"] = minPriceOverall;
    ViewData["MaxPriceOverall"] = maxPriceOverall;
    ViewData["InStock"] = inStock;
    ViewData["ItemsPerPage"] = itemsPerPage;

    return View(viewModels);
}
    
    public IActionResult Terms() => View();
    public IActionResult About() => View();
    public IActionResult Contact() => View();
    public IActionResult Privacy() => View();
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitReview(ReviewViewModel model)
    {
        if (User.Identity is { IsAuthenticated: false })
        {
            TempData["ErrorMessage"] = "Пожалуйста, войдите в систему, чтобы оставить отзыв.";
            return RedirectToAction("Product", new { id = model.ProductVariantId });
        }
        
        if (!ModelState.IsValid || model.Rating < 1 || model.Rating > 5)
        {
            TempData["ErrorMessage"] = "Пожалуйста, выберите оценку от 1 до 5.";
            return RedirectToAction("Product", new { id = model.ProductVariantId });
        }
        
        var userId = GetCurrentUserId();
        
        var variant = await context.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Id == model.ProductVariantId);

        if (variant == null)
        {
            TempData["ErrorMessage"] = "Товар не найден.";
            return RedirectToAction("Product", new { id = model.ProductVariantId });
        }
        
        var hasPurchased = await context.OrderItems
            .AnyAsync(oi => oi.Order.UserId == userId 
                            && oi.ProductVariantId == model.ProductVariantId 
                            && oi.Order.Status == "completed");

        if (!hasPurchased)
        {
            TempData["ErrorMessage"] = "Вы не можете оставить отзыв, так как не приобрели этот товар.";
            return RedirectToAction("Product", new { id = model.ProductVariantId });
        }

        try
        {
            var review = new Review
            {
                ProductId = variant.ProductId,
                UserId = userId,
                Rating = model.Rating,
                Comment = model.Comment,
                CreatedAt = DateTime.UtcNow,
                IsModerated = false 
            };

            context.Reviews.Add(review);
            await context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ваш отзыв успешно отправлен и ожидает модерации.";
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Произошла ошибка сервера. Пожалуйста, попробуйте снова позже.";
        }

        return RedirectToAction("Product", new { id = model.ProductVariantId });
    }
    
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }
}