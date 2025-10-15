using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStoreEll.Core.Infrastructure.Data;

namespace TechStoreEll.Web.Controllers;

[Route("[controller]/[action]")]
public class ExportController(AppDbContext context, IWebHostEnvironment env) : Controller
{
    [HttpGet]
    public async Task<IActionResult> ExportAudit(DateTime from, DateTime to)
    {
        from = DateTime.SpecifyKind(from, DateTimeKind.Utc);
        to   = DateTime.SpecifyKind(to, DateTimeKind.Utc);
        
        var jsonResult = await context.Database
            .SqlQueryRaw<string>("SELECT export_audit({0}, {1})::text AS \"Value\"", from, to)
            .FirstAsync();

        var bytes = System.Text.Encoding.UTF8.GetBytes(jsonResult);
        var fileName = $"audit_{from:yyyyMMdd_HHmmss}_{to:yyyyMMdd_HHmmss}.json";

        return File(bytes, "application/json", fileName);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestBackup(string? note)
    {
        var userId = GetCurrentUserId();
        
        if (note == null)
        {
            return RedirectToAction("AdminPanel", "Admin");
        }
        
        var backup = await context.Database
            .SqlQueryRaw<BackupResult>("SELECT * FROM sp_request_backup({0}, {1})", userId, note)
            .FirstAsync();

        
        var backupFolder = Path.Combine(env.WebRootPath, "backups");
        Directory.CreateDirectory(backupFolder);

        var cmd = backup.command.Replace("%DBNAME%", context.Database.GetDbConnection().Database);
        var fullPath = Path.Combine(backupFolder, Path.GetFileName(cmd.Split(" -f ")[1]));

        var psi = new System.Diagnostics.ProcessStartInfo("/Library/PostgreSQL/16/bin/pg_dump", $"-Fc -f \"{fullPath}\" -d {context.Database.GetDbConnection().Database} --no-owner --schema=public")
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };


        var proc = System.Diagnostics.Process.Start(psi)!;
        await proc.WaitForExitAsync();

        if (proc.ExitCode != 0)
        {
            var err = await proc.StandardError.ReadToEndAsync();
            return BadRequest($"Ошибка pg_dump: {err}");
        }

        TempData["Success"] = $"Бэкап создан: {Path.GetFileName(fullPath)}";

        return RedirectToAction("AdminPanel", "Admin");
    }
    
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }
}

public class BackupResult
{
    public int backup_id { get; set; }
    public string command { get; set; } = "";
}
