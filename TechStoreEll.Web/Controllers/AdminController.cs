using System.Text;
using Microsoft.AspNetCore.Mvc;
using TechStoreEll.Api.Attributes;
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
//         //take = Math.Clamp(take, 10, 1000);
//
//         var logs = await auditService.GetAuditLogsAsync(take);
//         var model = new AdminPanelViewModel
//         {
//             AuditLogs = logs,
//             Take = take // сохраним значение для отображения в форме
//         };
//         return View(model);
//     }
// }

public class AdminController(
    AuditLogService auditService,
    AnalyticsService analyticsService) : Controller
{
    [AuthorizeRole("Admin")]
    public async Task<IActionResult> AdminPanel(int take = 100)
    {
        var logs = await auditService.GetAuditLogsAsync(take);
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
        Console.WriteLine($"Фильтр: {startDate} - {endDate}");
        Console.WriteLine($"Используем: {start} - {end}");
        Console.WriteLine($"Start: {startDate:O}");
        Console.WriteLine($"End:   {endDate:O}");
        //var start = (startDate?.Date ?? DateTime.UtcNow.Date.AddDays(-30)).ToUniversalTime();
        //var end = (endDate?.Date ?? DateTime.UtcNow.Date).ToUniversalTime().AddDays(1).AddTicks(-1);
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
    public async Task<IActionResult> ExportAnalyticsCsv(
        DateTime? startDate = null,
        DateTime? endDate = null)
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

        // собираем данные в csvшку
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
}