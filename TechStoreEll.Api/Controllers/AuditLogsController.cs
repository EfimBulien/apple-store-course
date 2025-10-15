using TechStoreEll.Api.Attributes;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Services;
using TechStoreEll.Core.Services.IServices;

namespace TechStoreEll.Api.Controllers;

[AuthorizeRole("Admin")]
public class AuditLogsController(
    IGenericRepository<AuditLog> repository, 
    ILogger<AuditLogsController> logger) 
    : EntityController<AuditLog> (repository, logger);