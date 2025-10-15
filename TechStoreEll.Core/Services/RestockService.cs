using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TechStoreEll.Core.DTOs;
using TechStoreEll.Core.Infrastructure.Data;
using TechStoreEll.Core.Models;
using TechStoreEll.Core.Services.IServices;

namespace TechStoreEll.Core.Services;

public class RestockService(AppDbContext context) : IRestockService
{
    public async Task<List<string>> GetActiveWarehousesAsync()
    {
        return await context.Warehouses
            .Where(w => w.IsActive)
            .Select(w => w.Name)
            .ToListAsync();
    }

    public async Task<List<ProductVariantDto>> GetProductVariantsAsync()
    {
        return await context.ProductVariants
            .Include(pv => pv.Product)
            .Select(pv => new ProductVariantDto 
            { 
                Id = pv.Id, 
                Name = $"{pv.Product.Name} ({pv.VariantCode})" 
            })
            .ToListAsync();
    }


    public async Task<List<InventoryViewModel>> GetInventoryAsync()
    {
        return await context.Inventories
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
    }

    public async Task<List<InventoryMovementViewModel>> GetInventoryMovementsAsync(int take = 100)
    {
        return await context.InventoryMovements
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
    }

    public async Task RestockAsync(int userId, List<RestockItemModel> items)
    {
        if (items.Count == 0) 
            throw new InvalidOperationException("Список товаров пуст");

        var itemsJson = JsonSerializer.Serialize(items);

        await context.Database.ExecuteSqlRawAsync(
            "CALL sp_restock({0}, {1}::jsonb)",
            userId,
            itemsJson
        );
    }
}
