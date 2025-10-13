using Microsoft.EntityFrameworkCore;
using TechStoreEll.Api.Entities;
using TechStoreEll.Api.Infrastructure.Data;

namespace TechStoreEll.Core.Services;

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