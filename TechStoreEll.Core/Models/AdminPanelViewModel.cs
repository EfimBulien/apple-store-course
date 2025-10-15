using TechStoreEll.Core.Entities;

namespace TechStoreEll.Core.Models;

public class AdminPanelViewModel
{
    public List<AuditLog> AuditLogs { get; set; } = [];
    public int Take { get; set; } = 100;
}