using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStoreEll.Core.Infrastructure.Data;
using TechStoreEll.Web.Models;

namespace TechStoreEll.Web.Controllers;

public class HomeController(AppDbContext context, ILogger<HomeController> logger) : Controller
{
    // public async Task<IActionResult> Index(string sort = "date-desc")
    // {
    //     logger.LogInformation("Начало выполнения Index с параметром сортировки: {Sort}", sort);
    //     try
    //     {
    //         // Запрос для получения вариантов товаров с основным изображением и связанными данными
    //         var variantsQuery = context.ProductVariants
    //             .Where(pv => pv.Product.Active)
    //             .Include(pv => pv.Product)
    //                 .ThenInclude(p => p.Category)
    //             .Include(pv => pv.ProductImages.Where(pi => pi.IsPrimary))
    //             .Select(pv => new ProductViewModel
    //             {
    //                 Id = pv.Id,
    //                 Sku = pv.VariantCode,
    //                 Name = pv.Product.Name,
    //                 CategoryName = pv.Product.Category != null ? pv.Product.Category.Name : "Без категории",
    //                 Price = pv.Price,
    //                 PrimaryImageUrl = pv.ProductImages
    //                     .Where(pi => pi.IsPrimary)
    //                     .OrderBy(pi => pi.SortOrder)
    //                     .Select(pi => pi.ImageUrl)
    //                     .FirstOrDefault() ?? "",
    //                 PrimaryImageAltText = pv.ProductImages
    //                     .Where(pi => pi.IsPrimary)
    //                     .OrderBy(pi => pi.SortOrder)
    //                     .Select(pi => pi.AltText)
    //                     .FirstOrDefault() ?? "",
    //                 AvgRating = pv.Product.AvgRating,
    //                 ReviewsCount = pv.Product.ReviewsCount,
    //                 CreatedAt = pv.Product.CreatedAt
    //             });
    //         
    //         variantsQuery = sort switch
    //         {
    //             "price-asc" => variantsQuery.OrderBy(p => p.Price ?? decimal.MaxValue),
    //             "price-desc" => variantsQuery.OrderByDescending(p => p.Price ?? decimal.MinValue),
    //             "rating-asc" => variantsQuery.OrderBy(p => p.AvgRating ?? 0),
    //             "rating-desc" => variantsQuery.OrderByDescending(p => p.AvgRating ?? 5),
    //             "date-asc" => variantsQuery.OrderBy(p => p.CreatedAt),
    //             "date-desc" => variantsQuery.OrderByDescending(p => p.CreatedAt),
    //             _ => variantsQuery.OrderByDescending(p => p.CreatedAt) 
    //         };
    //
    //         var variants = await variantsQuery.ToListAsync();
    //         logger.LogInformation("Успешно получено {VariantCount} вариантов товаров", variants.Count);
    //
    //         ViewData["CurrentSort"] = sort;
    //         return View(variants);
    //     }
    //     catch (Exception ex)
    //     {
    //         logger.LogError(ex, "Ошибка при получении вариантов товаров с сортировкой {Sort}", sort);
    //         throw;
    //     }
    // }
    
    public async Task<IActionResult> Index(string sort = "date-desc")
{
    // Получаем все варианты с продуктами
    var variants = await context.ProductVariants
        .Where(pv => pv.Product.Active)
        .Include(pv => pv.Product)
            .ThenInclude(p => p.Category)
        .Include(pv => pv.Product)
            .ThenInclude(p => p.Reviews) // ← подгружаем отзывы
        .Include(pv => pv.ProductImages.Where(pi => pi.IsPrimary))
        .ToListAsync();

    // Преобразуем в ViewModel с пересчётом агрегатов
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
            Price = pv.Price,
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

    // Сортировка в памяти (после пересчёта)
    viewModels = sort switch
    {
        "price-asc" => viewModels.OrderBy(p => p.Price ?? decimal.MaxValue).ToList(),
        "price-desc" => viewModels.OrderByDescending(p => p.Price ?? decimal.MinValue).ToList(),
        "rating-asc" => viewModels.OrderBy(p => p.AvgRating ?? 0).ToList(),
        "rating-desc" => viewModels.OrderByDescending(p => p.AvgRating ?? 0).ToList(),
        "date-asc" => viewModels.OrderBy(p => p.CreatedAt).ToList(),
        "date-desc" => viewModels.OrderByDescending(p => p.CreatedAt).ToList(),
        _ => viewModels.OrderByDescending(p => p.CreatedAt).ToList()
    };

    ViewData["CurrentSort"] = sort;
    return View(viewModels);
}

    public async Task<ViewResult> Privacy()
    {
        logger.LogInformation("Начало выполнения Privacy");
        try
        {
            var roles = await context.Roles.ToListAsync();
            logger.LogInformation("Успешно получено {RoleCount} ролей", roles.Count);
            return View(roles);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при получении ролей");
            throw;
        }
    }
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
            var avgRating = reviewsCount > 0 
                ? (decimal?)approvedReviews.Average(r => r.Rating) 
                : null;

            var vm = new ProductDetailViewModel
            {
                Id = variant.Id,
                Sku = variant.VariantCode,
                Name = variant.Product.Name,
                CategoryName = variant.Product.Category?.Name ?? "Без категории",
                Description = variant.Product.Description,
                Price = variant.Price,
                AvgRating = avgRating,          // ← пересчитано!
                ReviewsCount = reviewsCount,    // ← пересчитано!
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
                            ? $"{r.User.FirstName} {r.User.LastName?.FirstOrDefault()}."
                            : "Покупатель",
                        CreatedAt = r.CreatedAt,
                        Rating = r.Rating,
                        Comment = r.Comment
                    })
                    .ToList()
            };

            return View(vm);
        }
    
}
