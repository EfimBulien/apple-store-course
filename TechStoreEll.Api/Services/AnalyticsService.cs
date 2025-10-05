using Microsoft.EntityFrameworkCore;
using TechStoreEll.Api.Models;
using TechStoreEll.Core.Infrastructure.Data;

namespace TechStoreEll.Api.Services;

public class AnalyticsService(AppDbContext context)
{
    public async Task<List<SalesByDay>> GetSalesByDayAsync(DateTime start, DateTime end)
    {
        return await context.Orders
            .Where(o => o.CreatedAt >= start && o.CreatedAt <= end)
            .GroupBy(o => DateOnly.FromDateTime(o.CreatedAt))
            .Select(g => new SalesByDay
            {
                Date = g.Key,
                Total = g.Sum(o => o.TotalAmount)
            })
            .OrderBy(x => x.Date)
            .ToListAsync();
    }

    public async Task<List<TopCategory>> GetTopCategoriesAsync(DateTime start, DateTime end)
    {
        var orderItems = await context.OrderItems
            .Include(oi => oi.ProductVariant)
            .ThenInclude(pv => pv.Product)
            .ThenInclude(p => p.Category)
            .Where(oi => oi.Order.CreatedAt >= start && oi.Order.CreatedAt <= end)
            .ToListAsync();

        return orderItems
            .Where(oi => oi.ProductVariant?.Product?.Category != null)
            .GroupBy(oi => oi.ProductVariant.Product.Category.Name)
            .Select(g => new TopCategory
            {
                CategoryName = g.Key,
                Revenue = g.Sum(oi => oi.UnitPrice * oi.Quantity)
            })
            .OrderByDescending(x => x.Revenue)
            .Take(5)
            .ToList();
    }

    public async Task<List<TopProduct>> GetTopProductsAsync(DateTime start, DateTime end)
    {
        return await context.OrderItems
            .Where(oi => oi.Order.CreatedAt >= start && oi.Order.CreatedAt <= end)
            .Join(context.ProductVariants, oi => oi.ProductVariantId, pv => pv.Id, (oi, pv) => new { oi, pv })
            .Join(context.Products, x => x.pv.ProductId, p => p.Id, (x, p) => new { x.oi, p })
            .GroupBy(x => x.p.Name)
            .Select(g => new TopProduct
            {
                ProductName = g.Key,
                QuantitySold = g.Sum(x => x.oi.Quantity)
            })
            .OrderByDescending(x => x.QuantitySold)
            .Take(5)
            .ToListAsync();
    }

    public async Task<List<UserActivity>> GetUserActivityAsync(DateTime start, DateTime end)
    {
        return await context.Orders
            .Where(o => o.CreatedAt >= start && o.CreatedAt <= end)
            .GroupBy(o => new { o.User.FirstName, o.User.LastName })
            .Select(g => new UserActivity
            {
                FullName = $"{g.Key.FirstName} {g.Key.LastName}",
                OrderCount = g.Count()
            })
            .OrderByDescending(x => x.OrderCount)
            .Take(10)
            .ToListAsync();
    }
}