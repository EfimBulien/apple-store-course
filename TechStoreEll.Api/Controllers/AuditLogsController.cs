using TechStoreEll.Api.Infrastructure.Data;
using TechStoreEll.Api.Models;

namespace TechStoreEll.Api.Controllers;

public class AuditLogsController(AppDbContext context, ILogger<EntityController<AuditLog>> logger) :
    EntityController<AuditLog>(context, logger);