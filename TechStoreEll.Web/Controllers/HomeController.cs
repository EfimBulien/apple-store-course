using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStoreEll.Api.Entities;
using TechStoreEll.Api.Infrastructure.Data;
using TechStoreEll.Web.Models;

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

        // только ОДОБРЕННЫЕ отзывы
        var approvedReviews = variant.Product.Reviews
            .Where(r => r.IsModerated)
            .ToList();

        // агрегаты ТОЛЬКО по одобренным отзывам
        var reviewsCount = approvedReviews.Count;
        var avgRating = reviewsCount > 0 ? (decimal?)approvedReviews.Average(r => r.Rating) : null;

        var vm = new ProductDetailViewModel
        {
            Id = variant.Id,
            Sku = variant.VariantCode,
            Name = variant.Product.Name,
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
    int take = 100, 
    int skip = 10,
    int? categoryId = null,
    int? minRam = null,
    int? maxRam = null,
    int? minStorage = null,
    int? maxStorage = null)
{
    var query = context.ProductVariants
        .Where(pv => pv.Product.Active);

    // Поиск
    if (!string.IsNullOrWhiteSpace(q))
    {
        var cleanQuery = q.Trim();
        query = query.Where(pv => 
            pv.Product.SearchVector.Matches(EF.Functions.PlainToTsQuery("russian", cleanQuery))
        );
    }

    // Фильтрация по категории
    if (categoryId.HasValue && categoryId > 0)
    {
        query = query.Where(pv => pv.Product.CategoryId == categoryId);
    }

    // Фильтрация по оперативной памяти
    if (minRam.HasValue)
    {
        query = query.Where(pv => pv.Ram >= minRam.Value);
    }
    if (maxRam.HasValue)
    {
        query = query.Where(pv => pv.Ram <= maxRam.Value);
    }

    // Фильтрация по постоянной памяти
    if (minStorage.HasValue)
    {
        query = query.Where(pv => pv.StorageGb >= minStorage.Value);
    }
    if (maxStorage.HasValue)
    {
        query = query.Where(pv => pv.StorageGb <= maxStorage.Value);
    }

    var variants = await query
        .Include(pv => pv.Product)
        .ThenInclude(p => p.Category)
        .Include(pv => pv.Product)
        .ThenInclude(p => p.Reviews)
        .Include(pv => pv.ProductImages.Where(pi => pi.IsPrimary))
        .Take(take)
        .ToListAsync();

    var viewModels = variants.Select(pv =>
    {
        var approvedReviews = pv.Product.Reviews.Where(r => r.IsModerated).ToList();
        var reviewsCount = approvedReviews.Count;
        var avgRating = reviewsCount > 0 ? (decimal?)approvedReviews.Average(r => r.Rating) : null;

        return new ProductViewModel
        {
            Id = pv.Id,
            Sku = pv.VariantCode,
            Name = pv.Product.Name,
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
            CreatedAt = pv.Product.CreatedAt
        };
    }).ToList();

    viewModels = sort switch
    {
        "price-asc" => viewModels.OrderBy(p => p.Price).ToList(),
        "price-desc" => viewModels.OrderByDescending(p => p.Price).ToList(),
        "rating-asc" => viewModels.OrderBy(p => p.AvgRating ?? 0).ToList(),
        "rating-desc" => viewModels.OrderByDescending(p => p.AvgRating ?? 0).ToList(),
        "date-asc" => viewModels.OrderBy(p => p.CreatedAt).ToList(),
        "date-desc" => viewModels.OrderByDescending(p => p.CreatedAt).ToList(),
        _ => viewModels.OrderByDescending(p => p.CreatedAt).ToList()
    };

    // Получаем категории для фильтра
    var categories = await context.Categories
        .OrderBy(c => c.Name)
        .ToListAsync();

    ViewData["CurrentSearch"] = q;
    ViewData["CurrentSort"] = sort;
    ViewData["Categories"] = categories;
    ViewData["SelectedCategoryId"] = categoryId;
    ViewData["MinRam"] = minRam;
    ViewData["MaxRam"] = maxRam;
    ViewData["MinStorage"] = minStorage;
    ViewData["MaxStorage"] = maxStorage;

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