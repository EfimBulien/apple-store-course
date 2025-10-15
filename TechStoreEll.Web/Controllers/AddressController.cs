using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TechStoreEll.Core.Models;
using TechStoreEll.Core.Services;
using TechStoreEll.Web.Helpers;

namespace TechStoreEll.Web.Controllers;

[AuthorizeRole("Customer")]
public class AddressController(AddressService addressService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserId();
        var addresses = await addressService.GetUserAddressesAsync(userId);
        var customer = await addressService.GetCustomerAsync(userId);

        ViewBag.ShippingAddressId = customer?.ShippingAddressId;
        ViewBag.BillingAddressId = customer?.BillingAddressId;

        return View(addresses);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        var userId = GetCurrentUserId();
        var model = id.HasValue
            ? await addressService.GetAddressFormAsync(id.Value, userId)
            : new AddressFormModel();

        if (id.HasValue && model == null)
            return NotFound();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AddressFormModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = GetCurrentUserId();
        await addressService.SaveAddressAsync(model, userId);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();

        try
        {
            await addressService.DeleteAddressAsync(id, userId);
            TempData["Success"] = "Адрес удалён.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetShipping(int id)
    {
        var userId = GetCurrentUserId();

        try
        {
            await addressService.SetShippingAddressAsync(userId, id);
            TempData["Success"] = "Адрес доставки обновлён.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetBilling(int id)
    {
        var userId = GetCurrentUserId();

        try
        {
            await addressService.SetBillingAddressAsync(userId, id);
            TempData["Success"] = "Платёжный адрес обновлён.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }
}
