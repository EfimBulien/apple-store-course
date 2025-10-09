using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStoreEll.Api.Attributes;
using TechStoreEll.Api.Entities;
using TechStoreEll.Core.Infrastructure.Data;
using TechStoreEll.Web.Models;

namespace TechStoreEll.Web.Controllers;

[AuthorizeRole("Customer")]
public class AddressController(AppDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserId();
        var addresses = await context.Addresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
        
        var customer = await context.Customers
            .FirstOrDefaultAsync(c => c.Id == userId);

        ViewBag.ShippingAddressId = customer?.ShippingAddressId;
        ViewBag.BillingAddressId = customer?.BillingAddressId;

        return View(addresses);
    }
    
    public async Task<IActionResult> Edit(int? id)
    {
        var userId = GetCurrentUserId();
        AddressFormModel model;

        if (id.HasValue)
        {
            var address = await context.Addresses
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
            if (address == null) return NotFound();

            model = new AddressFormModel
            {
                Id = address.Id,
                Label = address.Label,
                Country = address.Country,
                Region = address.Region,
                City = address.City,
                Street = address.Street,
                House = address.House,
                Apartment = address.Apartment,
                Postcode = address.Postcode
            };
        }
        else
        {
            model = new AddressFormModel();
        }

        return View(model);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AddressFormModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = GetCurrentUserId();

        if (model.Id.HasValue)
        {
            var address = await context.Addresses
                .FirstOrDefaultAsync(a => a.Id == model.Id && a.UserId == userId);
            if (address == null) return NotFound();

            address.Label = model.Label;
            address.Country = model.Country;
            address.Region = model.Region;
            address.City = model.City;
            address.Street = model.Street;
            address.House = model.House;
            address.Apartment = model.Apartment;
            address.Postcode = model.Postcode;
        }
        else
        {
            var address = new Address
            {
                UserId = userId,
                Label = model.Label,
                Country = model.Country,
                Region = model.Region,
                City = model.City,
                Street = model.Street,
                House = model.House,
                Apartment = model.Apartment,
                Postcode = model.Postcode,
                CreatedAt = DateTime.UtcNow
            };
            context.Addresses.Add(address);
        }

        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();
        var address = await context.Addresses
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (address == null) return NotFound();
        
        var customer = await context.Customers
            .FirstOrDefaultAsync(c => c.Id == userId);

        if (customer?.ShippingAddressId == id || customer?.BillingAddressId == id)
        {
            TempData["Error"] = "Нельзя удалить адрес, используемый по умолчанию.";
            return RedirectToAction(nameof(Index));
        }

        context.Addresses.Remove(address);
        await context.SaveChangesAsync();

        TempData["Success"] = "Адрес удалён.";
        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetShipping(int id)
    {
        var userId = GetCurrentUserId();
        var address = await context.Addresses.FindAsync(id);
        if (address == null || address.UserId != userId)
            return NotFound();

        var customer = await context.Customers.FirstOrDefaultAsync(c => c.Id == userId);
        if (customer == null)
        {
            customer = new Customer { Id = userId, ShippingAddressId = id };
            context.Customers.Add(customer);
        }
        else
        {
            customer.ShippingAddressId = id;
        }

        await context.SaveChangesAsync();
        TempData["Success"] = "Адрес доставки обновлён.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetBilling(int id)
    {
        var userId = GetCurrentUserId();
        var address = await context.Addresses.FindAsync(id);
        if (address == null || address.UserId != userId)
            return NotFound();

        var customer = await context.Customers.FirstOrDefaultAsync(c => c.Id == userId);
        if (customer == null)
        {
            customer = new Customer { Id = userId, BillingAddressId = id };
            context.Customers.Add(customer);
        }
        else
        {
            customer.BillingAddressId = id;
        }

        await context.SaveChangesAsync();
        TempData["Success"] = "Платёжный адрес обновлён.";
        return RedirectToAction(nameof(Index));
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }
}