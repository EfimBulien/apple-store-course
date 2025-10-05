using TechStoreEll.Api.Attributes;
using TechStoreEll.Api.Models;
using TechStoreEll.Api.Services;

namespace TechStoreEll.Api.Controllers;

[AuthorizeRole("Admin")]
public class AuditLogsController(
    IGenericRepository<AuditLog> repository, 
    ILogger<AuditLogsController> logger) 
    : EntityController<AuditLog> (repository, logger);