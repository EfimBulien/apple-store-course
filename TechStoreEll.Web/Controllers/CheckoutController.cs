using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;
using TechStoreEll.Api.Attributes;
using TechStoreEll.Api.Entities;
using TechStoreEll.Api.Infrastructure.Data;
using TechStoreEll.Web.Models;
using Order = TechStoreEll.Api.Entities.Order;

namespace TechStoreEll.Web.Controllers;

[AuthorizeRole("Customer", "Admin")]
public class CheckoutController(IConnectionMultiplexer redis, AppDbContext context) : Controller
{
    private readonly IDatabase _redis = redis.GetDatabase();

    private string GetCartKey()
    {
        var userName = User.Identity?.Name ?? throw new NullReferenceException();
        
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        return $"cart:{userId}:{userName}";
    }
    
    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserId();
        
        var addresses = await context.Addresses
            .Where(a => a.UserId == userId)
            .ToListAsync();
        
        var customer = await context.Customers
            .FirstOrDefaultAsync(c => c.Id == userId);
       
        var model = new CheckoutModel
        {
            ShippingAddressId = customer?.ShippingAddressId,
            BillingAddressId = customer?.BillingAddressId ?? customer?.ShippingAddressId,
            UseSameAddress = customer?.BillingAddressId == customer?.ShippingAddressId
        };

        ViewBag.Addresses = addresses;
        return View(model);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateOrder(CheckoutModel model)
    {
        var userId = GetCurrentUserId();
        var cartKey = GetCartKey();
        var cartData = await _redis.StringGetAsync(cartKey);

        if (string.IsNullOrEmpty(cartData))
        {
            ModelState.AddModelError("", "Корзина пуста");
            return await LoadAndReturnIndexView(userId, model);
        }

        var cart = JsonSerializer.Deserialize<List<CartItemViewModel>>(cartData!);
        if (cart == null || cart.Count == 0)
        {
            ModelState.AddModelError("", "Корзина пуста");
            return await LoadAndReturnIndexView(userId, model);
        }

        if (!model.ShippingAddressId.HasValue)
        {
            ModelState.AddModelError("", "Укажите адрес доставки");
            return await LoadAndReturnIndexView(userId, model);
        }

        if (model.UseSameAddress)
        {
            model.BillingAddressId = model.ShippingAddressId;
        }
        else if (!model.BillingAddressId.HasValue)
        {
            ModelState.AddModelError("", "Укажите платёжный адрес");
            return await LoadAndReturnIndexView(userId, model);
        }
        
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var variantIds = cart
                .Select(i => i.ProductId)
                .ToList();
            
            var variants = await context.ProductVariants
                .Where(pv => variantIds.Contains(pv.Id))
                .ToDictionaryAsync(pv => pv.Id, pv => pv);

            if (variants.Count != cart.Count)
            {
                throw new InvalidOperationException("Некоторые товары недоступны");
            }
            
            var inventories = await context.Inventories
                .Where(i => variantIds.Contains(i.ProductVariantId))
                .ToListAsync();

            foreach (var item in from item in cart let available = inventories
                         .Where(i => i.ProductVariantId == item.ProductId)
                         .Sum(i => i.Quantity - i.Reserve) where available < item.Quantity select item)
            {
                throw new InvalidOperationException($"Недостаточно товара: {item.Name}");
            }
            
            var orderNumber = "ORD"
                              + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + 
                              new Random().Next(1000, 9999);
            
            var totalAmount = cart.Sum(i => i.Price * i.Quantity);
            var order = new Order
            {
                OrderNumber = orderNumber,
                UserId = userId,
                Status = "new",
                TotalAmount = (decimal)totalAmount!,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Orders.Add(order);
            await context.SaveChangesAsync();
            
            foreach (var item in cart)
            {
                context.OrderItems.Add(new OrderItem
                {
                    OrderId = order.Id,
                    ProductVariantId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = (decimal)item.Price!
                });
                
                var remainingQty = item.Quantity;
                var variantInventories = inventories
                    .Where(i => i.ProductVariantId == item.ProductId)
                    .OrderByDescending(i => i.Quantity) // как в процедуре
                    .ToList();

                foreach (var inv in variantInventories)
                {
                    if (remainingQty <= 0) break;

                    var availableNow = inv.Quantity - inv.Reserve;
                    if (availableNow <= 0) continue;

                    var reserveQty = Math.Min(remainingQty, availableNow);
                    inv.Reserve += reserveQty;
                    context.Inventories.Update(inv);

                    remainingQty -= reserveQty;
                }

                if (remainingQty > 0)
                    throw new InvalidOperationException($"Не удалось зарезервировать весь товар: {item.Name}");
                
                var firstInventory = variantInventories.FirstOrDefault();
                if (firstInventory != null)
                {
                    context.InventoryMovements.Add(new InventoryMovement
                    {
                        ProductVariantId = item.ProductId,
                        WarehouseId = (int)firstInventory.WarehouseId!,
                        ChangeQty = -item.Quantity, // отрицательное = в резерв
                        Reason = $"зарезервировано для заказа #{order.Id}",
                        CreatedBy = userId,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            
            // можно в дальнейшем оплатить
            context.Payments.Add(new Payment
            {
                OrderId = order.Id,
                Provider = "placeholder",
                Amount = (decimal)totalAmount,
                Status = "pending",
                // Изменмть модель потом 
                // добавить CreatedAt = DateTime.UtcNow
                //PaidAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            await _redis.KeyDeleteAsync(cartKey);
            
            TempData["Success"] = $"Заказ #{order.Id} успешно создан!";
            return RedirectToAction("Details", "Order", new { id = order.Id });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError("", $"Ошибка: {ex.Message}");
            return await LoadAndReturnIndexView(userId, model);
        }
    }
    
    private async Task<IActionResult> LoadAndReturnIndexView(int userId, CheckoutModel model)
    {
        var addresses = await context.Addresses
            .Where(a => a.UserId == userId)
            .ToListAsync();
        
        ViewBag.Addresses = addresses;
        return View("Index", model);
    }
    
    private int GetCurrentUserId()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userId ?? "0");
    }
}