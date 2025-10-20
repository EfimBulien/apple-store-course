using TechStoreEll.Core.DTOs;
using TechStoreEll.Core.Models;

namespace TechStoreEll.Core.Interfaces;

public interface IRestockService
{
    Task<List<string>> GetActiveWarehousesAsync();
    Task<List<ProductVariantDto>> GetProductVariantsAsync();
    Task<List<InventoryViewModel>> GetInventoryAsync();
    Task<List<InventoryMovementViewModel>> GetInventoryMovementsAsync(int take = 100);
    Task RestockAsync(int userId, List<RestockItemModel> items);
}