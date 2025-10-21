using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Infrastructure.Data;

namespace TechStoreEll.Web.Controllers;

[Route("[controller]/[action]")]
public class ExportController(AppDbContext context, IWebHostEnvironment env) : Controller
{
    [HttpGet]
    public async Task<IActionResult> ExportAudit(DateTime from, DateTime to)
    {
        from = DateTime.SpecifyKind(from, DateTimeKind.Utc);
        to = DateTime.SpecifyKind(to, DateTimeKind.Utc);

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
            return RedirectToAction("BackupHistory");

        var dbUser = "postgres";
        var dbPassword = "1008";
        var dbName = "TechStoreEll";
        var dbHost = "localhost";
        var dbPort = "5432";

        var backupFolder = Path.Combine(env.WebRootPath, "backups");
        Directory.CreateDirectory(backupFolder);

        var fileName = $"backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}.dump";
        var fullPath = Path.Combine(backupFolder, fileName);

        var command = $"-h {dbHost} -p {dbPort} -U {dbUser} -Fc -f \"{fullPath}\" -d {dbName} --no-owner --schema=public";

        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "/Library/PostgreSQL/16/bin/pg_dump",
            Arguments = command,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            Environment =
            {
                ["PGPASSWORD"] = dbPassword
            }
        };

        using var proc = System.Diagnostics.Process.Start(psi)!;
        await proc.WaitForExitAsync();

        if (proc.ExitCode != 0)
        {
            var err = await proc.StandardError.ReadToEndAsync();
            return BadRequest($"Ошибка pg_dump: {err}");
        }

        var backup = new Backup
        {
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            Filename = fileName,
            Command = $"{psi.FileName} {command}",
            Note = note
        };

        context.Backups.Add(backup);
        await context.SaveChangesAsync();

        TempData["Success"] = $"Бэкап успешно создан: {fileName}";
        return RedirectToAction("BackupHistory");
    }

    [HttpGet]
    public async Task<IActionResult> BackupHistory()
    {
        var backups = await context.Backups
            .Include(b => b.CreatedByNavigation)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return View(backups);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> DownloadBackup(int id)
    {
        var backup = await context.Backups.FindAsync(id);
        if (backup == null)
            return NotFound();

        var filePath = Path.Combine(env.WebRootPath, "backups", backup.Filename ?? "");
        if (!System.IO.File.Exists(filePath))
            return NotFound("Файл не найден");

        var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
        return File(fileBytes, "application/octet-stream", backup.Filename);
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var id) ? id : 0;
    }
}
