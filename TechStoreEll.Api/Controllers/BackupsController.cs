using TechStoreEll.Api.Attributes;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Services;
using TechStoreEll.Core.Services.IServices;

namespace TechStoreEll.Api.Controllers;

[AuthorizeRole("Admin")]
public class BackupsController(
    IGenericRepository<Backup> repository,
    ILogger<BackupsController> logger)
    : EntityController<Backup>(repository, logger);
