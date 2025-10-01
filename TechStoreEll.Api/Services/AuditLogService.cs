using Microsoft.EntityFrameworkCore;
using TechStoreEll.Api.Infrastructure.Data;
using TechStoreEll.Api.Models;

namespace TechStoreEll.Api.Services;

public class AuditLogService(AppDbContext context)
{
    public async Task<List<AuditLog>> GetAuditLogsAsync(int take = 100)
    {
        return await context.AuditLogs
            .Include(a => a.ChangedByNavigation)
            .OrderByDescending(a => a.ChangedAt)
            .Take(take)
            .ToListAsync();
    }
}