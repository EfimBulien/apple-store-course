using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TechStoreEll.Core.Interfaces;
using TechStoreEll.Core.Models;
using TechStoreEll.Web.Helpers;

namespace TechStoreEll.Web.Controllers;

[AuthorizeRole("Customer")]
public class CartController(ICartService cartService) : Controller
{
    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new NullReferenceException();
    private string GetUserName() => User.Identity?.Name ?? throw new NullReferenceException();

    public async Task<IActionResult> Index()
    {
        var cart = await cartService.GetCartAsync(GetUserId(), GetUserName());
        return View(cart);
    }

    public async Task<IActionResult> Add(int productId, string name, decimal price, string imageUrl, int quantity = 1)
    {
        var item = new CartItemViewModel
        {
            ProductId = productId,
            Name = name,
            Price = price,
            Quantity = quantity,
            ImageUrl = imageUrl
        };

        await cartService.AddItemAsync(GetUserId(), GetUserName(), item);
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Update(int productId, int quantity)
    {
        await cartService.UpdateItemAsync(GetUserId(), GetUserName(), productId, quantity);
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Remove(int productId)
    {
        await cartService.RemoveItemAsync(GetUserId(), GetUserName(), productId);
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Clear()
    {
        await cartService.ClearCartAsync(GetUserId(), GetUserName());
        return RedirectToAction("Index");
    }

    [HttpGet("/cart/json")]
    public async Task<IActionResult> GetCartJson()
    {
        var cart = await cartService.GetCartAsync(GetUserId(), GetUserName());
        return Json(cart);
    }
}