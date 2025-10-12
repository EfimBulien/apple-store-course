using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStoreEll.Api.Attributes;
using TechStoreEll.Core.Infrastructure.Data;
using TechStoreEll.Web.Models;

namespace TechStoreEll.Web.Controllers;

[AuthorizeRole("Admin")]
public class RestockController(AppDbContext context) : Controller
{
    public async Task<IActionResult> Index(int take = 100)
    {
        var warehouses = await context.Warehouses
            .Where(w => w.IsActive)
            .Select(w => w.Name)
            .ToListAsync();

        var variants = await context.ProductVariants
            .Include(pv => pv.Product)
            .Select(pv => new
            {
                pv.Id,
                Name = $"{pv.Product.Name} ({pv.VariantCode})"
            })
            .ToListAsync();
        
        var inventory = await context.Inventories
            .Include(i => i.ProductVariant)
            .ThenInclude(pv => pv.Product)
            .Include(i => i.Warehouse)
            .Select(i => new InventoryViewModel
            {
                ProductName = i.ProductVariant.Product.Name,
                VariantCode = i.ProductVariant.VariantCode,
                WarehouseName = i.Warehouse != null ? i.Warehouse.Name : "—",
                Quantity = i.Quantity,
                Reserve = i.Reserve
            })
            .ToListAsync();
        
        var movements = await context.InventoryMovements
            .Include(m => m.ProductVariant)
            .ThenInclude(pv => pv.Product)
            .Include(m => m.Warehouse)
            .Include(m => m.CreatedByNavigation)
            .OrderByDescending(m => m.CreatedAt)
            .Take(take)
            .Select(m => new InventoryMovementViewModel
            {
                ProductName = m.ProductVariant.Product.Name,
                VariantCode = m.ProductVariant.VariantCode,
                WarehouseName = m.Warehouse.Name,
                ChangeQty = m.ChangeQty,
                Reason = m.Reason,
                CreatedBy = m.CreatedByNavigation != null 
                    ? $"{m.CreatedByNavigation.FirstName} {m.CreatedByNavigation.LastName}"
                    : "Система",
                CreatedAt = m.CreatedAt
            })
            .ToListAsync();

        ViewBag.Warehouses = warehouses;
        ViewBag.Variants = variants;

        var model = new RestockViewModel()
        {
            Inventory = inventory,
            Movements = movements
        };

        return View(model);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(RestockViewModel model)
    {
        if (model.Items.Count == 0)
        {
            ModelState.AddModelError("", "Добавьте хотя бы один товар.");
            return await Index();
        }

        if (!ModelState.IsValid)
        {
            return await Index();
        }
        
        var itemsJson = JsonSerializer.Serialize(model.Items);
        
        try
        {
            var userId = GetCurrentUserId();
            
            Console.WriteLine(userId + " " + itemsJson);
            
            await context.Database.ExecuteSqlRawAsync(
                "CALL sp_restock({0}, {1}::jsonb)",
                userId, 
                itemsJson
            );

            TempData["Success"] = "Склад успешно пополнен!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Ошибка при пополнении: {ex.Message}");
            return await Index();
        }
    }
    
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }
}