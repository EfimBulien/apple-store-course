using System.Globalization;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Entities.Views;
using TechStoreEll.Core.Infrastructure.Data;
using TechStoreEll.Core.Models;
using TechStoreEll.Core.Services;
using TechStoreEll.Web.Helpers;
using TechStoreEll.Web.Models;

namespace TechStoreEll.Web.Controllers;

[AuthorizeRole("Admin")]
public class AdminController(AuditLogService auditService, AnalyticsService analyticsService, AppDbContext context) : Controller
{
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }

    public IActionResult ReturnCsvFile(string content, string fileName)
    {
        var preamble = Encoding.UTF8.GetPreamble();
        var csvBytes = Encoding.UTF8.GetBytes(content);
        var resultBytes = new byte[preamble.Length + csvBytes.Length];
        Buffer.BlockCopy(preamble, 0, resultBytes, 0, preamble.Length);
        Buffer.BlockCopy(csvBytes, 0, resultBytes, preamble.Length, csvBytes.Length);
        return File(resultBytes, "text/csv", fileName);
    }
    
    [AuthorizeRole("Admin")]
    public async Task<IActionResult> ProductList()
    {
        var products = await context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductVariants)
            .ToListAsync();

        var model = products.Select(p => new ProductListItemViewModel
        {
            Id = p.Id,
            Name = p.Name!,
            Sku = p.Sku!,
            CategoryName = p.Category != null ? p.Category.Name : "Без категории",
            VariantCount = p.ProductVariants.Count,
            Active = p.Active
        }).ToList();

        return View(model);
    }
    
    [AuthorizeRole("Admin")]
    [HttpPost]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await context.Products
            .Include(p => p.ProductVariants)
            .ThenInclude(v => v.ProductImages)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound();

        foreach (var variant in product.ProductVariants) 
            context.ProductImages.RemoveRange(variant.ProductImages);
        
        context.ProductVariants.RemoveRange(product.ProductVariants);
        context.Products.Remove(product);

        await context.SaveChangesAsync();

        TempData["Message"] = $"Товар «{product.Name}» успешно удалён.";
        return RedirectToAction("ProductList");
    }
    
    [AuthorizeRole("Admin")]
    public async Task<IActionResult> EditProduct(int id)
    {
        var product = await context.Products
            .Include(p => p.ProductVariants)
            .ThenInclude(v => v.ProductImages)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return NotFound();

        var categories = await context.Categories.ToListAsync();

        var model = new ProductEditViewModel
        {
            Id = product.Id,
            Name = product.Name!,
            Sku = product.Sku!,
            Description = product.Description,
            Active = product.Active,
            CategoryId = (int)product.CategoryId!,
            Categories = categories,
            Variants = product.ProductVariants.Select(v => new ProductVariantEditModel
            {
                Id = v.Id,
                VariantCode = v.VariantCode,
                Price = v.Price,
                Color = v.Color,
                StorageGb = v.StorageGb,
                Ram = v.Ram,
                Images = v.ProductImages.Select(i => new ProductImageEditModel
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    AltText = i.AltText,
                    SortOrder = (int)i.SortOrder!,
                    IsPrimary = i.IsPrimary
                }).ToList()
            }).ToList()
        };

        return View(model);
    }
    [AuthorizeRole("Admin")]
    [HttpPost]
    public async Task<IActionResult> EditProduct(ProductEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Categories = await context.Categories.ToListAsync();
            foreach (var variant in model.Variants)
            {
                variant.Images ??= new List<ProductImageEditModel>();
            }
            return View(model);
        }

        try
        {
            var product = await context.Products
                .Include(p => p.ProductVariants)
                .ThenInclude(v => v.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == model.Id);

            if (product == null)
                return NotFound();
            
            product.Name = model.Name;
            product.Sku = model.Sku;
            product.Description = model.Description;
            product.Active = model.Active;
            product.CategoryId = model.CategoryId == 0 ? null : model.CategoryId;

            var existingVariantIds = product.ProductVariants.Select(v => v.Id).ToHashSet();
            var incomingVariantIds = model.Variants.Where(v => v.Id > 0).Select(v => v.Id).ToHashSet();

            var variantsToRemove = product.ProductVariants
                .Where(v => !incomingVariantIds.Contains(v.Id))
                .ToList();
                
            foreach (var variant in variantsToRemove)
            {
                context.ProductVariants.Remove(variant);
            }

            foreach (var variantModel in model.Variants)
            {
                ProductVariant variant;
                
                if (variantModel.Id > 0)
                {
                    variant = product.ProductVariants.FirstOrDefault(v => v.Id == variantModel.Id);
                    if (variant != null)
                    {
                        variant.VariantCode = variantModel.VariantCode;
                        variant.Price = variantModel.Price;
                        variant.Color = variantModel.Color;
                        variant.StorageGb = variantModel.StorageGb;
                        variant.Ram = variantModel.Ram;
                    }
                }
                else
                {
                    variant = new ProductVariant
                    {
                        ProductId = product.Id,
                        VariantCode = variantModel.VariantCode,
                        Price = variantModel.Price,
                        Color = variantModel.Color,
                        StorageGb = variantModel.StorageGb,
                        Ram = variantModel.Ram
                    };
                    context.ProductVariants.Add(variant);
                }

                if (variant == null) continue;

                if (variantModel.Images != null && variantModel.Images.Any())
                {
                    await ProcessVariantImages(variant, variantModel.Images);
                }
            }

            await context.SaveChangesAsync();
            TempData["Message"] = $"Товар «{model.Name}» успешно обновлён.";
            return RedirectToAction("EditProduct", new { id = model.Id });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обновлении товара: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            
            ModelState.AddModelError("", "Произошла ошибка при сохранении товара.");
            model.Categories = await context.Categories.ToListAsync();
            foreach (var variant in model.Variants)
            {
                variant.Images ??= new List<ProductImageEditModel>();
            }
            return View(model);
        }
    }

    private async Task ProcessVariantImages(ProductVariant variant, List<ProductImageEditModel> imageModels)
    {
        if (variant.Id == 0)
        {
            await context.SaveChangesAsync();
        }

        var existingImageIds = variant.ProductImages.Select(img => img.Id).ToHashSet();
        var incomingImageIds = imageModels.Where(img => img.Id > 0).Select(img => img.Id).ToHashSet();

        var imagesToRemove = variant.ProductImages
            .Where(img => !incomingImageIds.Contains(img.Id))
            .ToList();
            
        foreach (var img in imagesToRemove)
        {
            context.ProductImages.Remove(img);
        }

        foreach (var imgModel in imageModels)
        {
            if (imgModel.Id > 0)
            {
                var existingImg = variant.ProductImages.FirstOrDefault(i => i.Id == imgModel.Id);
                if (existingImg != null)
                {
                    existingImg.ImageUrl = imgModel.ImageUrl;
                    existingImg.AltText = imgModel.AltText;
                    existingImg.SortOrder = imgModel.SortOrder;
                    existingImg.IsPrimary = imgModel.IsPrimary;
                }
            }
            else
            {
                var newImage = new ProductImage
                {
                    ProductVariantId = variant.Id,
                    ImageUrl = imgModel.ImageUrl,
                    AltText = imgModel.AltText,
                    SortOrder = imgModel.SortOrder,
                    IsPrimary = imgModel.IsPrimary
                };
                context.ProductImages.Add(newImage);
            }
        }
    }
    
    [AuthorizeRole("Admin")]
    [HttpPost]
    public async Task<IActionResult> ApproveReview(int id)
    {
        var review = await context.Reviews.FindAsync(id);
        if (review == null) 
            return NotFound();

        review.IsModerated = true;
        review.ModeratedBy = GetCurrentUserId();

        await context.SaveChangesAsync();
        return RedirectToAction("ReviewModeration");
    }

    [AuthorizeRole("Admin")]
    [HttpPost]
    public async Task<IActionResult> RejectReview(int id)
    {
        var review = await context.Reviews.FindAsync(id);
        if (review == null) 
            return NotFound();

        context.Reviews.Remove(review);
        await context.SaveChangesAsync();
        
        return RedirectToAction(nameof(ReviewModeration));
    }
    
    public async Task<IActionResult> ReviewModeration(int page = 1, int pageSize = 20)
    {
        var reviews = await context.Reviews
            .Include(r => r.Product)
            .Include(r => r.User)
            .Where(r => !r.IsModerated)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalUnmoderated = await context.Reviews.CountAsync(r => !r.IsModerated);

        var model = new ReviewModerationViewModel
        {
            Reviews = reviews,
            Page = page,
            PageSize = pageSize,
            TotalUnmoderated = totalUnmoderated,
            TotalPages = (int)Math.Ceiling((double)totalUnmoderated / pageSize)
        };

        return View(model);
    }
    
    
    [AuthorizeRole("Admin")]
    public async Task<IActionResult> AdminPanel(int take = 100)
    {
        //take = Math.Clamp(take, 10, 1000);
        
        var logs = await auditService.GetAuditLogsAsync(take);
        var unmoderatedCount = await context.Reviews.CountAsync(r => !r.IsModerated);

        ViewBag.UnmoderatedCount = unmoderatedCount;
        var model = new AdminPanelViewModel
        {
            AuditLogs = logs,
            Take = take
        };
        
        return View(model);
    }
    
    [AuthorizeRole("Admin")]
    public async Task<IActionResult> Analytics(DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate?.Date ?? DateTime.UtcNow.Date.AddDays(-30);
        var end = (endDate?.Date ?? DateTime.UtcNow.Date).AddDays(1).AddTicks(-1);
        
        start = DateTime.SpecifyKind(start, DateTimeKind.Utc);
        end = DateTime.SpecifyKind(end, DateTimeKind.Utc);
        
        start = DateTime.SpecifyKind(start.Date, DateTimeKind.Utc);
        end = DateTime.SpecifyKind(end.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
        
        var model = new AnalyticsViewModel
        {
            StartDate = start,
            EndDate = endDate?.Date ?? DateTime.UtcNow,
            SalesByDay = await analyticsService.GetSalesByDayAsync(start, end),
            TopCategories = await analyticsService.GetTopCategoriesAsync(start, end),
            TopProducts = await analyticsService.GetTopProductsAsync(start, end),
            UserActivities = await analyticsService.GetUserActivityAsync(start, end)
        };

        return View(model);
    }
    
    [AuthorizeRole("Admin")]
    public IActionResult Export() => View("Export");

    [AuthorizeRole("Admin")]
    public async Task<IActionResult> ExportAnalyticsCsv(DateTime? startDate = null, DateTime? endDate = null)
    {
        const string separator = ";";
        
        var start = (startDate?.Date ?? DateTime.UtcNow.Date.AddDays(-30)).ToUniversalTime();
        var end = (endDate?.Date ?? DateTime.UtcNow.Date).ToUniversalTime().AddDays(1).AddTicks(-1);

        start = DateTime.SpecifyKind(start.Date, DateTimeKind.Utc);
        end = DateTime.SpecifyKind(end, DateTimeKind.Utc);

        var sales = await analyticsService.GetSalesByDayAsync(start, end);
        var categories = await analyticsService.GetTopCategoriesAsync(start, end);
        var products = await analyticsService.GetTopProductsAsync(start, end);
        var users = await analyticsService.GetUserActivityAsync(start, end);

        var csv = new StringBuilder();
        
        var preamble = Encoding.UTF8.GetPreamble();
        csv.AppendLine("Продажи по дням");
        csv.AppendLine($"Дата{separator}Сумма");
        foreach (var s in sales)
            csv.AppendLine($"{s.Date:dd.MM.yyyy}{separator}{s.Total:F2}");

        csv.AppendLine();
        csv.AppendLine("Топ категорий");
        csv.AppendLine($"Категория{separator}Выручка");
        foreach (var c in categories)
            csv.AppendLine($"{c.CategoryName}{separator}{c.Revenue:F2}");

        csv.AppendLine();
        csv.AppendLine("Топ товаров");
        csv.AppendLine($"Товар{separator}Продано шт.");
        foreach (var p in products)
            csv.AppendLine($"{p.ProductName}{separator}{p.QuantitySold}");

        csv.AppendLine();
        csv.AppendLine("Активность пользователей");
        csv.AppendLine($"Пользователь{separator}Количество заказов");
        foreach (var u in users)
            csv.AppendLine($"{u.FullName}{separator}{u.OrderCount}");

        var fileName = $"аналитика_{DateTime.UtcNow:yyyyMMdd}.csv";
        
        var csvBytes = Encoding.UTF8.GetBytes(csv.ToString());
        var resultBytes = new byte[preamble.Length + csvBytes.Length];
        Buffer.BlockCopy(preamble, 0, resultBytes, 0, preamble.Length);
        Buffer.BlockCopy(csvBytes, 0, resultBytes, preamble.Length, csvBytes.Length);

        return File(resultBytes, "text/csv", fileName);
    }
    
    [AuthorizeRole("Admin")]
    public IActionResult ImportProducts()
    {
        return View();
    }
    
    [AuthorizeRole("Admin")]
    public async Task<IActionResult> ExportProductsCsv()
    {
        var products = await context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductVariants)
            .ToListAsync();

        var csv = new StringBuilder();
        const string sep = ";";

        csv.AppendLine("Товары и варианты");
        csv.AppendLine($"ID товара{sep}Название{sep}Артикул (SKU){sep}Категория{sep}Описание{sep}Активен{sep}" +
                       $"ID варианта{sep}Код варианта{sep}Цена{sep}Цвет{sep}Память (ГБ){sep}ОЗУ (ГБ)");

        foreach (var p in products)
        {
            if (!p.ProductVariants.Any())
            {
                csv.AppendLine(
                    $"{p.Id}{sep}{p.Name}{sep}{p.Sku}{sep}{p.Category?.Name ?? "—"}{sep}" +
                    $"{(string.IsNullOrEmpty(p.Description) ? "—" : p.Description.Replace("\n", " ")
                        .Replace("\r", ""))}{sep}" +
                    $"{(p.Active ? "Да" : "Нет")}{sep}—{sep}—{sep}—{sep}—{sep}—{sep}—"
                );
            }
            else
            {
                foreach (var v in p.ProductVariants)
                {
                    csv.AppendLine(
                        $"{p.Id}{sep}{p.Name}{sep}{p.Sku}{sep}{p.Category?.Name ?? "—"}{sep}" +
                        $"{(string.IsNullOrEmpty(p.Description) ? "—" : p.Description.Replace("\n", " ")
                            .Replace("\r", ""))}{sep}" +
                        $"{(p.Active ? "Да" : "Нет")}{sep}" +
                        $"{v.Id}{sep}{v.VariantCode}{sep}{v.Price:F2}{sep}{v.Color ?? "—"}{sep}" +
                        $"{(v.StorageGb ?? 0)}{sep}{(v.Ram ?? 0)}"
                    );
                }
            }
        }

        return ReturnCsvFile(csv.ToString(), $"товары_варианты_{DateTime.UtcNow:yyyyMMdd}.csv");
    }
    
    [AuthorizeRole("Admin")]
    public async Task<IActionResult> ExportInventoryCsv()
    {
        var inventoryRecords = await context.Inventories
            .Include(i => i.ProductVariant)
            .ThenInclude(v => v.Product)
            .ThenInclude(p => p.Category)
            .Include(i => i.Warehouse)
            .ToListAsync();

        var csv = new StringBuilder();
        const string sep = ";";

        csv.AppendLine("Складские остатки");
        csv.AppendLine($"Товар{sep}SKU{sep}Вариант{sep}Склад{sep}Доступно{sep}Зарезервировано{sep}Итого (доступно)");

        foreach (var i in inventoryRecords)
        {
            var available = Math.Max(0, i.Quantity - i.Reserve);
            csv.AppendLine(
                $"{i.ProductVariant.Product.Name}{sep}" +
                $"{i.ProductVariant.Product.Sku}{sep}" +
                $"{i.ProductVariant.VariantCode}{sep}" +
                $"{i.Warehouse?.Name ?? "—"}{sep}" +
                $"{i.Quantity}{sep}" +
                $"{i.Reserve}{sep}" +
                $"{available}"
            );
        }

        return ReturnCsvFile(csv.ToString(), $"склад_остатки_{DateTime.UtcNow:yyyyMMdd}.csv");
    }
    
    [AuthorizeRole("Admin")]
    public async Task<IActionResult> ExportOrdersCsv()
    {
        var orders = await context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.ProductVariant)
            .ThenInclude(v => v.Product)
            .ToListAsync();

        var csv = new StringBuilder();
        const string sep = ";";

        csv.AppendLine("Заказы");
        csv.AppendLine($"ID заказа{sep}Покупатель{sep}Дата{sep}Статус{sep}Сумма (руб.){sep}Товар{sep}Кол-во (шт.){sep}Цена за ед. (руб.)");

        foreach (var o in orders)
        {
            foreach (var item in o.OrderItems)
            {
                var productInfo = $"{item.ProductVariant.Product.Name} [{item.ProductVariant.VariantCode}]";
                csv.AppendLine(
                    $"{o.Id}{sep}" +
                    $"{o.User.FirstName} {o.User.MiddleName} {o.User.LastName}{sep}" +
                    $"{o.CreatedAt:dd.MM.yyyy HH:mm}{sep}" +
                    $"{o.Status}{sep}" +
                    $"{o.TotalAmount:F2}{sep}" +
                    $"{productInfo}{sep}" +
                    $"{item.Quantity}{sep}" +
                    $"{item.UnitPrice:F2}"
                );
            }
        }

        return ReturnCsvFile(csv.ToString(), $"заказы_{DateTime.UtcNow:yyyyMMdd}.csv");
    }
    
    [AuthorizeRole("Admin")]
    [HttpPost]
    public async Task<IActionResult> ImportProducts(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Файл не выбран.");

        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var categories = await context.Categories
                .ToDictionaryAsync(c => c.Name, c => c.Id);

            using var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8);
            var header = await reader.ReadLineAsync();
            if (header == null)
                throw new InvalidOperationException("Файл пуст или не содержит заголовков.");

            var importedProducts = 0;
            var importedVariants = 0;

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(';');
                if (parts.Length < 10)
                    throw new InvalidOperationException($"Неверный формат строки: ожидалось минимум 10 колонок, получено {parts.Length}.");

                var name = parts[0].Trim();
                var sku = parts[1].Trim();
                var categoryName = parts[2].Trim();
                var description = parts[3].Trim();
                var active = parts[4].Trim().Equals("Да", StringComparison.OrdinalIgnoreCase);
                var variantCode = parts[5].Trim();
                var priceStr = parts[6].Trim();
                var color = string.IsNullOrWhiteSpace(parts[7]) ? null : parts[7].Trim();
                var storageStr = parts[8].Trim();
                var ramStr = parts[9].Trim();
                
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(sku) || string.IsNullOrEmpty(variantCode))
                    throw new InvalidOperationException($"Пропущены обязательные поля в строке: {line}");
                
                var price = decimal.TryParse(priceStr.Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture, out var p) ? p : throw new InvalidOperationException($"Некорректная цена: {priceStr}");
                var storage = string.IsNullOrEmpty(storageStr) ? (int?)null : int.TryParse(storageStr, out var s) ? s : throw new InvalidOperationException($"Некорректное значение памяти: {storageStr}");
                var ram = string.IsNullOrEmpty(ramStr) ? (int?)null : int.TryParse(ramStr, out var r) ? r : throw new InvalidOperationException($"Некорректное значение ОЗУ: {ramStr}");
                
                if (!categories.TryGetValue(categoryName, out var categoryId))
                {
                    var newCat = new Category { Name = categoryName };
                    context.Categories.Add(newCat);
                    await context.SaveChangesAsync();
                    categories[categoryName] = newCat.Id;
                    categoryId = newCat.Id;
                }

                var product = await context.Products.FirstOrDefaultAsync(product1 => product1.Sku == sku);
                if (product == null)
                {
                    product = new Product
                    {
                        Name = name,
                        Sku = sku,
                        CategoryId = categoryId,
                        Description = string.IsNullOrEmpty(description) ? null : description,
                        Active = active,
                        CreatedAt = DateTime.UtcNow
                    };
                    context.Products.Add(product);
                    await context.SaveChangesAsync();
                    importedProducts++;
                }
                else
                {
                    product.Name = name;
                    product.CategoryId = categoryId;
                    product.Description = string.IsNullOrEmpty(description) ? null : description;
                    product.Active = active;
                }
                
                var existingVariant = await context.ProductVariants
                    .FirstOrDefaultAsync(v => v.ProductId == product.Id && v.VariantCode == variantCode);

                if (existingVariant != null)
                {
                    existingVariant.Price = price;
                    existingVariant.Color = color;
                    existingVariant.StorageGb = storage;
                    existingVariant.Ram = ram;
                }
                else
                {
                    context.ProductVariants.Add(new ProductVariant
                    {
                        ProductId = product.Id,
                        VariantCode = variantCode,
                        Price = price,
                        Color = color,
                        StorageGb = storage,
                        Ram = ram
                    });
                    importedVariants++;
                }
            }
            
            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Message"] = $"✅ Успешно импортировано: {importedProducts} товаров, {importedVariants} вариантов.";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            TempData["ErrorMessage"] = $"❌ Ошибка импорта: {ex.Message}";
            return RedirectToAction("ImportProducts");
        }

        return RedirectToAction("Export");
    }
}