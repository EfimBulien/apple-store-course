using Microsoft.AspNetCore.Mvc;
using TechStoreEll.Api.Attributes;
using TechStoreEll.Core.Services;
using TechStoreEll.Web.Models;

namespace TechStoreEll.Web.Controllers;

public class AdminController(AuditLogService auditService) : Controller
{
    [AuthorizeRole("Admin")]
    public async Task<IActionResult> AdminPanel(int take = 100)
    {
        //take = Math.Clamp(take, 10, 1000);

        var logs = await auditService.GetAuditLogsAsync(take);
        var model = new AdminPanelViewModel
        {
            AuditLogs = logs,
            Take = take // сохраним значение для отображения в форме
        };
        return View(model);
    }
}