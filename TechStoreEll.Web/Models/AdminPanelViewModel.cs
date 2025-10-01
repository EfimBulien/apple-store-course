using TechStoreEll.Api.Models;

namespace TechStoreEll.Web.Models;

public class AdminPanelViewModel
{
    public List<AuditLog> AuditLogs { get; set; } = [];
    public int Take { get; set; } = 100;
}