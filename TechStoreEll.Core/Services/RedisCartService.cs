using StackExchange.Redis;
using System.Text.Json;
using TechStoreEll.Core.Interfaces;
using TechStoreEll.Core.Models;
using IDatabase = StackExchange.Redis.IDatabase;

namespace TechStoreEll.Core.Services;

public class RedisCartService(IConnectionMultiplexer redis) : ICartService
{
    private readonly IDatabase _redis = redis.GetDatabase();

    private static string GetCartKey(string userId, string userName) => $"cart:{userId}:{userName}";

    public async Task<List<CartItemViewModel>> GetCartAsync(string userId, string userName)
    {
        var cartKey = GetCartKey(userId, userName);
        var cartData = await _redis.StringGetAsync(cartKey);
        return string.IsNullOrEmpty(cartData) ? []
            : JsonSerializer.Deserialize<List<CartItemViewModel>>(cartData!) ?? new List<CartItemViewModel>();
    }

    public async Task AddItemAsync(string userId, string userName, CartItemViewModel item)
    {
        var cart = await GetCartAsync(userId, userName);

        var existingItem = cart.FirstOrDefault(c => c.ProductId == item.ProductId);
        if (existingItem != null)
        {
            existingItem.Quantity += item.Quantity;
        }
        else
        {
            cart.Add(item);
        }

        await _redis.StringSetAsync(GetCartKey(userId, userName), JsonSerializer.Serialize(cart));
    }

    public async Task UpdateItemAsync(string userId, string userName, int productId, int quantity)
    {
        var cart = await GetCartAsync(userId, userName);
        var item = cart.FirstOrDefault(c => c.ProductId == productId);
        if (item != null)
        {
            item.Quantity = quantity > 0 ? quantity : 1;
            await _redis.StringSetAsync(GetCartKey(userId, userName), JsonSerializer.Serialize(cart));
        }
    }

    public async Task RemoveItemAsync(string userId, string userName, int productId)
    {
        var cart = await GetCartAsync(userId, userName);
        cart.RemoveAll(c => c.ProductId == productId);
        await _redis.StringSetAsync(GetCartKey(userId, userName), JsonSerializer.Serialize(cart));
    }

    public async Task ClearCartAsync(string userId, string userName)
    {
        await _redis.KeyDeleteAsync(GetCartKey(userId, userName));
    }
}
