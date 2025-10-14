using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStoreEll.Api.Attributes;
using TechStoreEll.Api.Entities;
using TechStoreEll.Api.Infrastructure.Data;
using TechStoreEll.Api.Services;
using TechStoreEll.Web.Models;

namespace TechStoreEll.Web.Controllers;

[AuthorizeRole("Admin")]
public class ProductController(
    AppDbContext context, 
    IMinioService minioService, 
    ILogger<ProductController> logger)
    : Controller
{
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new ProductCreateViewModel
        {
            Categories = await context.Categories
                
                .OrderBy(c => c.Name)
                .ToListAsync()
        };

        
        model.Variants.Add(new ProductVariantCreateModel());

        return View(model);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductCreateViewModel model)
    {
        logger.LogWarning("=== ENTER Create POST ===");
        logger.LogWarning("ModelState.IsValid = {Valid}", ModelState.IsValid);
        logger.LogWarning("Variants = {Variants}", model.Variants == null ? "null" : model.Variants.Count.ToString());

        if (!ModelState.IsValid)
        {
            logger.LogWarning("ModelState invalid. Returning View.");
            model.Categories = await context.Categories.OrderBy(c => c.Name).ToListAsync();
            return View(model);
        }

        
        logger.LogInformation("Create POST: Variants count = {Count}", model.Variants?.Count ?? 0);
        if (model.Variants != null)
        {
            for (int i = 0; i < model.Variants.Count; i++)
            {
                logger.LogInformation("Variant #{Index}: Code='{Code}', Price={Price}, Images={ImagesCount}",
                    i, model.Variants[i]?.VariantCode, model.Variants[i]?.Price, model.Variants[i]?.Images?.Count ?? 0);
            }
        }

        
        if (model.Variants == null || !model.Variants.Any(v => !string.IsNullOrWhiteSpace(v.VariantCode)))
        {
            ModelState.AddModelError("", "Нужно добавить хотя бы один вариант с кодом.");
            model.Categories = await context.Categories.OrderBy(c => c.Name).ToListAsync();
            return View(model);
        }

        
        var uploadedObjectNames = new List<string>();

        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            
            var product = new Product
            {
                Sku = model.Sku?.Trim(),
                Name = model.Name?.Trim(),
                CategoryId = model.CategoryId,
                Description = model.Description?.Trim(),
                Active = model.Active,
                CreatedAt = DateTime.UtcNow,
                
                
            };

            
            foreach (var variantModel in model.Variants)
            {
                if (variantModel == null || string.IsNullOrWhiteSpace(variantModel.VariantCode))
                    continue; 

                var variant = new ProductVariant
                {
                    VariantCode = variantModel.VariantCode.Trim(),
                    Price = variantModel.Price,
                    Color = variantModel.Color?.Trim(),
                    StorageGb = variantModel.StorageGb,
                    Ram = variantModel.Ram
                    
                };

                
                
                product.GetType().GetProperty("ProductVariants")?.GetValue(product); 

                
                if (variantModel.Images?.Any() == true)
                {
                    var sort = 0;
                    foreach (var file in variantModel.Images)
                    {
                        if (file == null || file.Length == 0) continue;

                        
                        
                        
                        var imageUrl = await minioService.UploadImageAsync(file);
                        if (string.IsNullOrWhiteSpace(imageUrl))
                            throw new InvalidOperationException("Minio returned empty image URL");

                        
                        var uri = new Uri(imageUrl);
                        var objectName = Path.GetFileName(uri.LocalPath);
                        if (!string.IsNullOrEmpty(objectName))
                            uploadedObjectNames.Add(objectName);

                        var productImage = new ProductImage
                        {
                            ImageUrl = imageUrl,
                            AltText = $"{product.Name} {variantModel.Color} - изображение {sort + 1}",
                            SortOrder = sort,
                            IsPrimary = sort == 0
                        };

                        
                        
                        var imagesColl = variant.GetType().GetProperty("ProductImages")?.GetValue(variant) as IList<ProductImage>;
                        if (imagesColl != null)
                        {
                            imagesColl.Add(productImage);
                        }
                        else
                        {
                            
                            
                        }

                        
                        
                        
                        if (variant.GetType().GetProperty("ProductImages")?.GetValue(variant) == null)
                        {
                            var prop = variant.GetType().GetProperty("ProductImages");
                            if (prop != null)
                            {
                                var listType = typeof(List<ProductImage>);
                                prop.SetValue(variant, Activator.CreateInstance(listType));
                                ((List<ProductImage>)prop.GetValue(variant)).Add(productImage);
                            }
                        }

                        sort++;
                    }
                }

                
                var pvColl = product.GetType().GetProperty("ProductVariants")?.GetValue(product) as IList<ProductVariant>;
                if (pvColl != null)
                {
                    pvColl.Add(variant);
                }
                else
                {
                    
                    var prop = product.GetType().GetProperty("ProductVariants");
                    if (prop != null)
                    {
                        var listType = typeof(List<ProductVariant>);
                        prop.SetValue(product, Activator.CreateInstance(listType));
                        ((List<ProductVariant>)prop.GetValue(product)).Add(variant);
                    }
                }
            }

            
            context.Products.Add(product);

            
            await context.SaveChangesAsync();

            
            await transaction.CommitAsync();

            TempData["Success"] = $"Товар '{model.Name}' успешно создан!";
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            
            try { await transaction.RollbackAsync(); } catch { /* ignore */ }

            
            foreach (var objectName in uploadedObjectNames)
            {
                try
                {
                    await minioService.DeleteImageAsync(objectName);
                }
                catch (Exception delEx)
                {
                    logger.LogWarning(delEx, "Не удалось удалить объект {ObjectName} из MinIO после ошибки", objectName);
                }
            }

            
            logger.LogError(ex, "Ошибка при создании товара. Все изменения отменены.");
            ModelState.AddModelError("", "Ошибка при создании товара. Все изменения отменены. Подробности в логах.");

            model.Categories = await context.Categories.OrderBy(c => c.Name).ToListAsync();
            return View(model);
        }
    }
    
    [HttpPost]
    public IActionResult AddVariantRow()
    {
        var variant = new ProductVariantCreateModel();
        return PartialView("_VariantEditor", variant);
    }
}