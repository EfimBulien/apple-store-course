using TechStoreEll.Api.Attributes;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Interfaces;
using TechStoreEll.Core.Services;

namespace TechStoreEll.Api.Controllers;

[AuthorizeRole("Admin")]
public class BackupsController(
    IGenericRepository<Backup> repository,
    ILogger<BackupsController> logger)
    : EntityController<Backup>(repository, logger);
