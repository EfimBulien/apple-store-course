using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Text.Json;
using TechStoreEll.Api.Attributes;
using TechStoreEll.Web.Models;

namespace TechStoreEll.Web.Controllers;

[AuthorizeRole("Customer", "Admin")]
public class CartController(IConnectionMultiplexer redis) : Controller
{
    private readonly IDatabase _redis = redis.GetDatabase();

    private string GetCartKey()
    {
        return $"cart:{HttpContext.User}";
    }
    
    public async Task<IActionResult> Index()
    {
        var cartKey = GetCartKey();
        var cartData = await _redis.StringGetAsync(cartKey);

        var cart = string.IsNullOrEmpty(cartData) ? []
            : JsonSerializer.Deserialize<List<CartItemViewModel>>(cartData!) ?? [];

        return View(cart);
    }
    
    public async Task<IActionResult> Add(int productId, string name, decimal price, string imageUrl, int quantity = 1)
    {
        var cartKey = GetCartKey();
        var cartData = await _redis.StringGetAsync(cartKey);
        var cart = string.IsNullOrEmpty(cartData) ? []
            : JsonSerializer.Deserialize<List<CartItemViewModel>>(cartData!) ?? [];

        var existingItem = cart.FirstOrDefault(c => c.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            cart.Add(new CartItemViewModel
            {
                ProductId = productId,
                Name = name,
                Price = price,
                Quantity = quantity,
                ImageUrl = imageUrl
            });
        }

        await _redis.StringSetAsync(cartKey, JsonSerializer.Serialize(cart));
        return RedirectToAction("Index");
    }
    
    public async Task<IActionResult> Remove(int productId)
    {
        var cartKey = GetCartKey();
        var cartData = await _redis.StringGetAsync(cartKey);
        var cart = string.IsNullOrEmpty(cartData) ? []
            : JsonSerializer.Deserialize<List<CartItemViewModel>>(cartData!) ?? [];

        cart.RemoveAll(c => c.ProductId == productId);

        await _redis.StringSetAsync(cartKey, JsonSerializer.Serialize(cart));
        return RedirectToAction("Index");
    }
    
    public async Task<IActionResult> Update(int productId, int quantity)
    {
        var cartKey = GetCartKey();
        var cartData = await _redis.StringGetAsync(cartKey);
        var cart = string.IsNullOrEmpty(cartData) ? []
            : JsonSerializer.Deserialize<List<CartItemViewModel>>(cartData!) ?? [];

        var item = cart.FirstOrDefault(c => c.ProductId == productId);
        if (item != null)
        {
            item.Quantity = quantity > 0 ? quantity : 1;
        }

        await _redis.StringSetAsync(cartKey, JsonSerializer.Serialize(cart));
        return RedirectToAction("Index");
    }
    
    public async Task<IActionResult> Clear()
    {
        var cartKey = GetCartKey();
        await _redis.KeyDeleteAsync(cartKey);
        return RedirectToAction("Index");
    }
    
    [HttpGet("json")]
    public async Task<IActionResult> GetCartJson()
    {
        var cartKey = GetCartKey();
        var cartData = await _redis.StringGetAsync(cartKey);
        var cart = string.IsNullOrEmpty(cartData) ? []
            : JsonSerializer.Deserialize<List<CartItemViewModel>>(cartData!) ?? [];
        return Json(cart);
    }
}
