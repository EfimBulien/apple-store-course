using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TechStoreEll.Core.Interfaces;
using TechStoreEll.Core.Models;
using TechStoreEll.Web.Helpers;

namespace TechStoreEll.Web.Controllers;

[AuthorizeRole("Admin")]
public class RestockController(IRestockService restockService) : Controller
{
    public async Task<IActionResult> Index(int take = 100)
    {
        var warehouses = await restockService.GetActiveWarehousesAsync();
        var variants = await restockService.GetProductVariantsAsync();
        var inventory = await restockService.GetInventoryAsync();
        var movements = await restockService.GetInventoryMovementsAsync(take);

        ViewBag.Warehouses = warehouses;
        ViewBag.Variants = variants;

        var model = new RestockViewModel
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

        try
        {
            var userId = GetCurrentUserId();
            await restockService.RestockAsync(userId, model.Items);

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
