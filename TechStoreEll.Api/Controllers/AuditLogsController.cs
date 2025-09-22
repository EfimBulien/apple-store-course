using TechStoreEll.Api.Data;
using TechStoreEll.Api.Models;

namespace TechStoreEll.Api.Controllers;

public class AuditLogsController(AppDbContext context) : EntityController<AuditLog>(context)
{
}
