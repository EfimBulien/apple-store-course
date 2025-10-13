using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStoreEll.Api.Attributes;
using TechStoreEll.Api.Infrastructure.Data;
using TechStoreEll.Api.Models;
using TechStoreEll.Api.Services;
using TechStoreEll.Core.Services;
using TechStoreEll.Web.Models;

namespace TechStoreEll.Web.Controllers;

// public class AdminController(AuditLogService auditService) : Controller
// {
//     [AuthorizeRole("Admin")]
//     public async Task<IActionResult> AdminPanel(int take = 100)
//     {
//         take = Math.Clamp(take, 10, 1000);
//
//         var logs = await auditService.GetAuditLogsAsync(take);
//         var model = new AdminPanelViewModel
//         {
//             AuditLogs = logs,
//             Take = take
//         };
//         return View(model);
//     }
// }

public class AdminController(AuditLogService auditService, AnalyticsService analyticsService, AppDbContext context) : Controller
{
    [AuthorizeRole("Admin")]
    public IActionResult Export() => View("Export");
    
    [AuthorizeRole("Admin")]
    [HttpPost]
    public async Task<IActionResult> ApproveReview(int id)
    {
        Console.WriteLine(id);
        var review = await context.Reviews.FindAsync(id);
        Console.WriteLine(review);
        if (review == null) return NotFound();

        review.IsModerated = true;
        review.ModeratedBy = GetCurrentUserId();

        
        //await UpdateProductRating(review.ProductId);

        await context.SaveChangesAsync();
        return RedirectToAction("ReviewModeration");
    }

    [AuthorizeRole("Admin")]
    [HttpPost]
    public async Task<IActionResult> RejectReview(int id)
    {
        var review = await context.Reviews.FindAsync(id);
        if (review == null) return NotFound();

        context.Reviews.Remove(review);
        await context.SaveChangesAsync();
        
        //await UpdateProductRating(review.ProductId);
        return RedirectToAction(nameof(ReviewModeration));
    }
    
    // private async Task UpdateProductRating(int productId)
    // {
    //     var product = await context.Products.FindAsync(productId);
    //     if (product == null) return;
    //
    //     var reviews = await context.Reviews
    //         .Where(r => r.ProductId == productId && r.IsModerated)
    //         .ToListAsync();
    //
    //     if (reviews.Any())
    //     {
    //         product.AvgRating = (decimal)reviews.Average(r => r.Rating);
    //         product.ReviewsCount = reviews.Count;
    //     }
    //     else
    //     {
    //         product.AvgRating = 0;
    //         product.ReviewsCount = 0;
    //     }
    //
    //     context.Products.Update(product);
    // }

    [AuthorizeRole("Admin")]
    public async Task<IActionResult> ReviewModeration(int page = 1, int pageSize = 20)
    {
        var reviews = await context.Reviews
            .Include(r => r.Product)
            .Include(r => r.User)
            .Where(r => !r.IsModerated)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalUnmoderated = await context.Reviews.CountAsync(r => !r.IsModerated);

        var model = new ReviewModerationViewModel
        {
            Reviews = reviews,
            Page = page,
            PageSize = pageSize,
            TotalUnmoderated = totalUnmoderated,
            TotalPages = (int)Math.Ceiling((double)totalUnmoderated / pageSize)
        };

        return View(model);
    }
    
    
    [AuthorizeRole("Admin")]
    public async Task<IActionResult> AdminPanel(int take = 100)
    {
        var logs = await auditService.GetAuditLogsAsync(take);
        var unmoderatedCount = await context.Reviews.CountAsync(r => !r.IsModerated);

        ViewBag.UnmoderatedCount = unmoderatedCount;
        var model = new AdminPanelViewModel
        {
            AuditLogs = logs,
            Take = take
        };
        return View(model);
    }
    
    [AuthorizeRole("Admin")]
    public async Task<IActionResult> Analytics(DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate?.Date ?? DateTime.UtcNow.Date.AddDays(-30);
        var end = (endDate?.Date ?? DateTime.UtcNow.Date).AddDays(1).AddTicks(-1);
        
        start = DateTime.SpecifyKind(start, DateTimeKind.Utc);
        end = DateTime.SpecifyKind(end, DateTimeKind.Utc);
        
        start = DateTime.SpecifyKind(start.Date, DateTimeKind.Utc);
        end = DateTime.SpecifyKind(end.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
        
        var model = new AnalyticsViewModel
        {
            StartDate = start,
            EndDate = endDate?.Date ?? DateTime.UtcNow,
            SalesByDay = await analyticsService.GetSalesByDayAsync(start, end),
            TopCategories = await analyticsService.GetTopCategoriesAsync(start, end),
            TopProducts = await analyticsService.GetTopProductsAsync(start, end),
            UserActivities = await analyticsService.GetUserActivityAsync(start, end)
        };

        return View(model);
    }

    [AuthorizeRole("Admin")]
    public async Task<IActionResult> ExportAnalyticsCsv(DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = (startDate?.Date ?? DateTime.UtcNow.Date.AddDays(-30)).ToUniversalTime();
        var end = (endDate?.Date ?? DateTime.UtcNow.Date).ToUniversalTime().AddDays(1).AddTicks(-1);

        start = DateTime.SpecifyKind(start.Date, DateTimeKind.Utc);
        end = DateTime.SpecifyKind(end, DateTimeKind.Utc);

        var sales = await analyticsService.GetSalesByDayAsync(start, end);
        var categories = await analyticsService.GetTopCategoriesAsync(start, end);
        var products = await analyticsService.GetTopProductsAsync(start, end);
        var users = await analyticsService.GetUserActivityAsync(start, end);

        var csv = new StringBuilder();
        
        csv.AppendLine("Sales by Day");
        csv.AppendLine("Date,Total");
        foreach (var s in sales)
            csv.AppendLine($"{s.Date:yyyy-MM-dd},{s.Total:F2}");

        csv.AppendLine();
        csv.AppendLine("Top Categories");
        csv.AppendLine("Category,Revenue");
        foreach (var c in categories)
            csv.AppendLine($"{c.CategoryName},{c.Revenue:F2}");

        csv.AppendLine();
        csv.AppendLine("Top Products");
        csv.AppendLine("Product,QuantitySold");
        foreach (var p in products)
            csv.AppendLine($"{p.ProductName},{p.QuantitySold}");

        csv.AppendLine();
        csv.AppendLine("User Activity");
        csv.AppendLine("User,OrderCount");
        foreach (var u in users)
            csv.AppendLine($"{u.FullName},{u.OrderCount}");

        var fileName = $"analytics_{DateTime.UtcNow:yyyyMMdd}.csv";
        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);
    }
    
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }
}