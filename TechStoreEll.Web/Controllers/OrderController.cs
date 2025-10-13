using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStoreEll.Api.Attributes;
using TechStoreEll.Api.Entities;
using TechStoreEll.Api.Infrastructure.Data;
using TechStoreEll.Web.Models;

namespace TechStoreEll.Web.Controllers;

[AuthorizeRole("Customer")]
public class OrderController(AppDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserId();

        var orders = await context.Orders
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderSummary
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                Status = o.Status,
                TotalAmount = o.TotalAmount,
                CreatedAt = o.CreatedAt,
                PaymentStatus = o.Payments.FirstOrDefault() != null ? o.Payments.First().Status : "Не оплачён",
                Items = o.OrderItems.Select(oi => new OrderItemSummary
                {
                    ProductName = oi.ProductVariant.Product.Name,
                    VariantInfo = GetVariantInfo(oi.ProductVariant),
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    ImageUrl = oi.ProductVariant.ProductImages.FirstOrDefault()!.ImageUrl
                }).ToList()
            })
            .ToListAsync();

        var model = new OrderViewModel
        {
            Orders = orders
        };
        
        return View(model);
    }
    
    public async Task<IActionResult> Details(int id)
    {
        var userId = GetCurrentUserId();

        var order = await context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.ProductVariant)
            .ThenInclude(pv => pv.Product)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.ProductVariant)
            .ThenInclude(pv => pv.ProductImages)
            .Include(o => o.Payments)
            .Where(o => o.UserId == userId && o.Id == id)
            .FirstOrDefaultAsync();

        if (order == null)
        {
            return NotFound();
        }
        
        var reviewedProductIds = await context.Reviews
            .Where(r => r.UserId == userId)
            .Select(r => r.ProductId)
            .ToHashSetAsync();

        var summary = new OrderSummary
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt,
            PaymentStatus = order.Payments.FirstOrDefault()?.Status ?? "Не оплачён",

            Items = order.OrderItems.Select(oi => new OrderItemSummary
            {
                ProductName = oi.ProductVariant.Product.Name,
                VariantInfo = GetVariantInfo(oi.ProductVariant),
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                ImageUrl = oi.ProductVariant.ProductImages
                               .FirstOrDefault(img => img.IsPrimary)?.ImageUrl ??
                           oi.ProductVariant.ProductImages.FirstOrDefault()?.ImageUrl
            }).ToList(),

            Reviews = order.OrderItems.Select(oi => new OrderItemWithReview
            {
                ProductName = oi.ProductVariant.Product.Name,
                VariantInfo = GetVariantInfo(oi.ProductVariant),
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                ImageUrl = oi.ProductVariant.ProductImages
                               .FirstOrDefault(img => img.IsPrimary)?.ImageUrl ??
                           oi.ProductVariant.ProductImages.FirstOrDefault()?.ImageUrl,
                ProductId = oi.ProductVariant.ProductId,
                AlreadyReviewed = reviewedProductIds.Contains(oi.ProductVariant.ProductId)
            }).ToList()
        };
        
        return View(summary);
    }

    private static string GetVariantInfo(ProductVariant pv)
    {
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(pv.Color))
        {
            parts.Add($"Цвет: {pv.Color}");
        }
        
        if (pv.StorageGb.HasValue)
        {
            parts.Add($"Память: {pv.StorageGb} ГБ");
        }
        
        if (pv.Ram.HasValue)
        {
            parts.Add($"ОЗУ: {pv.Ram} ГБ");
        }
        
        return parts.Count != 0 ? string.Join(", ", parts) : pv.VariantCode;
    }
    

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PayOrder(int id)
    {
        var userId = GetCurrentUserId();
        
        var order = await context.Orders
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (order is not { Status: "new" })
        {
            return NotFound();
        }

        var payment = order.Payments.FirstOrDefault();
        if (payment != null)
        {
            payment.Status = "success";
            payment.PaidAt = DateTime.UtcNow;
            payment.TransactionRef = "placeholder";
            context.Payments.Update(payment);
        }

        order.Status = "paid";
        order.UpdatedAt = DateTime.UtcNow;
        context.Orders.Update(order);
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteOrder(int id)
    {
        var userId = GetCurrentUserId();
        
        var order = await context.Orders.FindAsync(id);
        if (order == null || order.UserId != userId || order.Status != "paid")
        {
            return NotFound();
        }
        
        if (order.Status == "paid")
        {
            // вручную переводим заказ в shipped перед завершением
            order.Status = "shipped";
            await context.SaveChangesAsync();
        }
        
        await context.Database.ExecuteSqlRawAsync(
            "CALL sp_complete_order({0}, {1})",
            userId,
            id
        );

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LeaveReview(int orderId, int productId, int rating, string comment)
    {
        var userId = GetCurrentUserId();
        
        var order = await context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(orderItem => orderItem.ProductVariant)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId && o.Status == "completed");

        if (order == null || order.OrderItems.All(oi => oi.ProductVariant.ProductId != productId))
        {
            return BadRequest("Нельзя оставить отзыв");
        }
        
        var exists = await context.Reviews
            .AnyAsync(r => r.ProductId == productId && r.UserId == userId);
        
        if (exists)
        {
            return BadRequest("Отзыв уже оставлен");
        }

        context.Reviews.Add(new Review
        {
            ProductId = productId,
            UserId = userId,
            Rating = rating,
            Comment = comment,
            CreatedAt = DateTime.UtcNow,
            IsModerated = false
        });

        await context.SaveChangesAsync();
        TempData["Success"] = "Отзыв отправлен на модерацию.";
        return RedirectToAction(nameof(Details), new { id = orderId });
    }
}