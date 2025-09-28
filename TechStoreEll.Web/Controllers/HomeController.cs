using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStoreEll.Api.Infrastructure.Data;
using TechStoreEll.Web.Models;

namespace TechStoreEll.Web.Controllers;

public class HomeController(AppDbContext context, ILogger<HomeController> logger) : Controller
{
    public async Task<IActionResult> Index(string sort = "date-desc")
    {
        logger.LogInformation("Начало выполнения Index с параметром сортировки: {Sort}", sort);
        try
        {
            // Запрос для получения вариантов товаров с основным изображением и связанными данными
            var variantsQuery = context.ProductVariants
                .Where(pv => pv.Product.Active)
                .Include(pv => pv.Product)
                    .ThenInclude(p => p.Category)
                .Include(pv => pv.ProductImages.Where(pi => pi.IsPrimary))
                .Select(pv => new ProductViewModel
                {
                    Id = pv.Id,
                    Sku = pv.VariantCode,
                    Name = pv.Product.Name,
                    CategoryName = pv.Product.Category != null ? pv.Product.Category.Name : "Без категории",
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
                    AvgRating = pv.Product.AvgRating,
                    ReviewsCount = pv.Product.ReviewsCount,
                    CreatedAt = pv.Product.CreatedAt
                });
            
            variantsQuery = sort switch
            {
                "price-asc" => variantsQuery.OrderBy(p => p.Price ?? decimal.MaxValue),
                "price-desc" => variantsQuery.OrderByDescending(p => p.Price ?? decimal.MinValue),
                "rating-asc" => variantsQuery.OrderBy(p => p.AvgRating ?? 0),
                "rating-desc" => variantsQuery.OrderByDescending(p => p.AvgRating ?? 5),
                "date-asc" => variantsQuery.OrderBy(p => p.CreatedAt),
                "date-desc" => variantsQuery.OrderByDescending(p => p.CreatedAt),
                _ => variantsQuery.OrderByDescending(p => p.CreatedAt) 
            };

            var variants = await variantsQuery.ToListAsync();
            logger.LogInformation("Успешно получено {VariantCount} вариантов товаров", variants.Count);

            ViewData["CurrentSort"] = sort;
            return View(variants);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при получении вариантов товаров с сортировкой {Sort}", sort);
            throw;
        }
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
            .Include(v => v.ProductImages)
            .FirstOrDefaultAsync(v => v.Id == id);
    
        if (variant == null)
            return NotFound();
    
        var vm = new ProductDetailViewModel
        {
            Id = variant.Id,
            Sku = variant.VariantCode,
            Name = variant.Product.Name,
            CategoryName = variant.Product.Category?.Name ?? "Без категории",
            Description = variant.Product.Description,
            Price = variant.Price,
            AvgRating = variant.Product.AvgRating,
            ReviewsCount = variant.Product.ReviewsCount,
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
                .ToList()
        };
    
        return View(vm);
    }
}
