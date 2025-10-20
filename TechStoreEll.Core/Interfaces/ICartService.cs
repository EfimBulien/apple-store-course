using TechStoreEll.Core.Models;

namespace TechStoreEll.Core.Interfaces;

public interface ICartService
{
    Task<List<CartItemViewModel>> GetCartAsync(string userId, string userName);
    Task AddItemAsync(string userId, string userName, CartItemViewModel item);
    Task UpdateItemAsync(string userId, string userName, int productId, int quantity);
    Task RemoveItemAsync(string userId, string userName, int productId);
    Task ClearCartAsync(string userId, string userName);
}
