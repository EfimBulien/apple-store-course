using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Infrastructure.Data;

namespace TechStoreEll.Web.Controllers;

[Route("[controller]/[action]")]
public class ExportController(
    AppDbContext context,
    IWebHostEnvironment env,
    IConfiguration configuration) 
    : Controller
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

        var dbUser = configuration.GetValue<string>("Database:User") ?? "postgres";
        var dbPassword = configuration.GetValue<string>("Database:Password");
        var dbName = configuration.GetValue<string>("Database:Name") ?? "TechStoreEll";
        var dbHost = configuration.GetValue<string>("Database:Host") ?? "localhost";
        var dbPort = configuration.GetValue<string>("Database:Port") ?? "5432";

        if (string.IsNullOrEmpty(dbPassword))
        {
            return BadRequest("Пароль базы данных не настроен");
        }

        var backupFolder = Path.Combine(env.WebRootPath, "backups");
        Directory.CreateDirectory(backupFolder);

        var fileName = $"backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}.dump";
        var fullPath = Path.Combine(backupFolder, fileName);

        var pgDumpPath = FindPostgresTool("pg_dump");
        if (string.IsNullOrEmpty(pgDumpPath))
        {
            return BadRequest("pg_dump не найден в системе." +
                              " Убедитесь, что PostgreSQL установлен или измените конфигурационные файлы.");
        }

        var command = $"-h {dbHost} -p {dbPort} -U {dbUser} -Fc -f \"{fullPath}\" -d {dbName} --no-owner --schema=public";

        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = pgDumpPath,
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
    
    /// <summary>
    /// ВНИМАНИЕ!
    /// Это тестовый метод восстановления в базу данных
    /// Он создает новую базу данных и сравнивает их для проверки корректности переноса
    /// Закомментированные строки - Restore в продакшене
    /// ВНИМАНИЕ!
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestoreBackup(int id, string? confirmText)
    {
        if (confirmText != "ВОССТАНОВИТЬ")
        {
            TempData["Error"] = "Для подтверждения восстановления введите слово 'ВОССТАНОВИТЬ'";
            return RedirectToAction("BackupHistory");
        }

        var backup = await context.Backups.FindAsync(id);
        if (backup == null)
        {
            return NotFound("Бэкап не найден");
        }

        var filePath = Path.Combine(env.WebRootPath, "backups", backup.Filename ?? "");
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("Файл бэкапа не найден");
        }

        var dbUser = configuration.GetValue<string>("Database:User") ?? "postgres";
        var dbPassword = configuration.GetValue<string>("Database:Password");
        var dbName = configuration.GetValue<string>("Database:Name") ?? "TechStoreEll";
        var dbHost = configuration.GetValue<string>("Database:Host") ?? "localhost";
        var dbPort = configuration.GetValue<string>("Database:Port") ?? "5432";

        if (string.IsNullOrEmpty(dbPassword))
        {
            return BadRequest("Пароль базы данных не настроен");
        }

        var pgRestorePath = FindPostgresTool("pg_restore");
        if (string.IsNullOrEmpty(pgRestorePath))
        {
            return BadRequest("pg_restore не найден в системе. Убедитесь, что PostgreSQL установлен.");
        }

        //  УБЕРИТЕ КОММЕНТАРИИ ЕСЛИ ХОТИТЕ ВКЛЮЧИТЬ РЕАЛЬНОЕ ВОССТАНОВЛЕНИЕ
        try
        {
            //  УБЕРИ КОММЕНТИРОВАНИЕ ДЛЯ РЕАЛЬНОГО ВОССТАНОВЛЕНИЯ
            /*
            await TerminateDatabaseConnections(dbHost, dbPort, dbUser, dbPassword, dbName);

            var realCommand = $"-h {dbHost} -p {dbPort} -U {dbUser} -d {dbName} --clean --if-exists --no-owner -c \"{filePath}\"";

            var realPsi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = pgRestorePath,
                Arguments = realCommand,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Environment = { ["PGPASSWORD"] = dbPassword }
            };

            using var realProc = System.Diagnostics.Process.Start(realPsi)!;
            await realProc.WaitForExitAsync();

            if (realProc.ExitCode != 0)
            {
                var err = await realProc.StandardError.ReadToEndAsync();
                return BadRequest($"Ошибка pg_restore: {err}");
            }
            TempData["Success"] = $"База данных успешно восстановлена из бэкапа: {backup.Filename}";
            */
            
            var testDbName = $"test_restore_{DateTime.UtcNow:yyyyMMddHHmmss}";
            await ExecutePsqlCommand($"-h {dbHost} -p {dbPort} -U {dbUser} -d postgres -c \"CREATE DATABASE \\\"{testDbName}\\\";\"", dbPassword);
            
            // --clean --if-exists чтобы не было ошибок при удалении несуществующих объектов
            // и --no-owner чтобы не было проблем с владельцами
            var command = $"-h {dbHost} -p {dbPort} -U {dbUser} -d {testDbName} --clean --if-exists --no-owner -c \"{filePath}\"";

            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = pgRestorePath,
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
                // ТЕСТОВАЯ БАЗА ДАННЫХ УДАЛИТСЯ ПРИ ОШИБКЕ
                await ExecutePsqlCommand($"-h {dbHost} -p {dbPort} -U {dbUser} -d postgres -c \"DROP DATABASE IF EXISTS \\\"{testDbName}\\\";\"", dbPassword);
                return BadRequest($"Ошибка pg_restore: {err}");
            }

            // СРАВНЕНИЕ
            var currentTables = await GetTableCount(dbHost, dbPort, dbUser, dbPassword, dbName);
            var testTables = await GetTableCount(dbHost, dbPort, dbUser, dbPassword, testDbName);
            
            var currentRecords = await GetTotalRecordsCount(dbHost, dbPort, dbUser, dbPassword, dbName);
            var testRecords = await GetTotalRecordsCount(dbHost, dbPort, dbUser, dbPassword, testDbName);
            

            // ПО ЖЕЛАНИЮ МОЖНО УДАЛИТЬ БАЗУ
            // await ExecutePsqlCommand($"-h {dbHost} -p {dbPort} -U {dbUser} -d postgres -c \"DROP DATABASE IF EXISTS \\\"{testDbName}\\\";\"", dbPassword);
            
            TempData["Success"] = $"Тестовое восстановление успешно! " +
                                 $"Текущая база: {currentTables} таблиц, {currentRecords} записей. " +
                                 $"Бэкап: {testTables} таблиц, {testRecords} записей. " + 
                                 $"(Реальное восстановление отключено)";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Ошибка при восстановлении: {ex.Message}";
        }

        return RedirectToAction("BackupHistory");
    }

    private async Task<string> GetTableList(string host, string port, string user, string password, string dbName)
    {
        var result = await ExecutePsqlCommand(
            $"-h {host} -p {port} -U {user} -d {dbName} -c \"SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' ORDER BY table_name;\"", 
            password);
        
        var tables = result.Split('\n')
            .Where(line => !string.IsNullOrWhiteSpace(line) && !line.Contains("table_name") && !line.Contains("----") && !line.Contains("rows"))
            .Take(18)
            .ToArray();
        
        return tables.Length > 0 ? string.Join(", ", tables) : "нет таблиц";
    }
    
    private async Task<int> GetTableCount(string host, string port, string user, string password, string dbName)
    {
        var result = await ExecutePsqlCommand(
            $"-h {host} -p {port} -U {user} -d {dbName} -c \"SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public';\"", 
            password);
        
        return int.TryParse(result, out var count) ? count : 0;
    }

    private async Task<int> GetTotalRecordsCount(string host, string port, string user, string password, string dbName)
    {
        var result = await ExecutePsqlCommand(
            $"-h {host} -p {port} -U {user} -d {dbName} -c \"SELECT SUM(n_live_tup) FROM pg_stat_user_tables;\"", 
            password);
        
        return int.TryParse(result, out var count) ? count : 0;
    }

    private async Task<string> ExecutePsqlCommand(string arguments, string password)
    {
        var psqlPath = FindPostgresTool("psql");
        if (string.IsNullOrEmpty(psqlPath)) return string.Empty;

        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = psqlPath,
            Arguments = arguments,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            Environment = { ["PGPASSWORD"] = password }
        };

        using var proc = System.Diagnostics.Process.Start(psi)!;
        var output = await proc.StandardOutput.ReadToEndAsync();
        await proc.WaitForExitAsync();

        var lines = output.Split('\n');
        foreach (var line in lines)
        {
            if (int.TryParse(line.Trim(), out var result))
            {
                return result.ToString();
            }
        }

        return "0";
    }

    private async Task TerminateDatabaseConnections(
        string host,
        string port, 
        string user, 
        string password,
        string dbName)
    {
        var psqlPath = FindPostgresTool("psql");
        if (string.IsNullOrEmpty(psqlPath)) return;

        var terminateCommand = $"-h {host} -p {port} -U {user} -d postgres -c \"SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '{dbName}' AND pid <> pg_backend_pid();\"";

        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = psqlPath,
            Arguments = terminateCommand,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            Environment =
            {
                ["PGPASSWORD"] = password
            }
        };

        using var proc = System.Diagnostics.Process.Start(psi)!;
        await proc.WaitForExitAsync();
    }

    private string? FindPostgresTool(string toolName)
    {
        var executableName = OperatingSystem.IsWindows() ? $"{toolName}.exe" : toolName;

        var searchPaths = configuration.GetSection("PostgresPaths:SearchPaths").Get<List<string>>() ?? [];
        
        foreach (var path in searchPaths)
        {
            if (!string.IsNullOrEmpty(path))
            {
                var fullPath = Path.Combine(path, executableName);
                if (System.IO.File.Exists(fullPath))
                    return fullPath;
            }
        }

        var pathDirs = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? [];
        foreach (var dir in pathDirs)
        {
            if (string.IsNullOrEmpty(dir)) continue;
            
            var fullPath = Path.Combine(dir, executableName);
            if (System.IO.File.Exists(fullPath))
                return fullPath;
        }

        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = OperatingSystem.IsWindows() ? "where" : "which",
                Arguments = executableName,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = System.Diagnostics.Process.Start(psi);
            if (proc != null)
            {
                proc.WaitForExit(2000);
                if (proc.ExitCode == 0)
                {
                    var output = proc.StandardOutput.ReadToEnd().Trim();
                    if (!string.IsNullOrEmpty(output))
                    {
                        var firstLine = output.Split('\n')[0].Trim();
                        if (System.IO.File.Exists(firstLine))
                            return firstLine;
                    }
                }
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }

        return null;
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