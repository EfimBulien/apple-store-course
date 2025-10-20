using TechStoreEll.Api.Attributes;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Interfaces;
using TechStoreEll.Core.Services;

namespace TechStoreEll.Api.Controllers;

[AuthorizeRole("Admin")]
public class AuditLogsController(
    IGenericRepository<AuditLog> repository, 
    ILogger<AuditLogsController> logger) 
    : EntityController<AuditLog> (repository, logger);