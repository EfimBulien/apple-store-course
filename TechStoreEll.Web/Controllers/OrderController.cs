using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStoreEll.Api.Attributes;
using TechStoreEll.Api.Entities;
using TechStoreEll.Core.Infrastructure.Data;
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
                PaymentStatus = o.Payments.FirstOrDefault() != null 
                    ? o.Payments.First().Status 
                    : "Не оплачён",
                Items = o.OrderItems.Select(oi => new OrderItemSummary
                {
                    ProductName = oi.ProductVariant.Product.Name,
                    VariantInfo = GetVariantInfo(oi.ProductVariant),
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    ImageUrl = oi.ProductVariant.ProductImages.FirstOrDefault().ImageUrl
                }).ToList()
            })
            .ToListAsync();

        var model = new OrderHistoryViewModel { Orders = orders };
        return View(model);
    }

    // Детали заказа (опционально)
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

        if (order == null) return NotFound();

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
            }).ToList()
        };

        return View(summary);
    }

    private static string GetVariantInfo(ProductVariant pv)
    {
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(pv.Color)) parts.Add($"Цвет: {pv.Color}");
        if (pv.StorageGb.HasValue) parts.Add($"Память: {pv.StorageGb} ГБ");
        if (pv.Ram.HasValue) parts.Add($"ОЗУ: {pv.Ram} ГБ");
        return parts.Count != 0 ? string.Join(", ", parts) : pv.VariantCode;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }
}