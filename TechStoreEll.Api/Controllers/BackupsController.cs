using TechStoreEll.Api.Attributes;
using TechStoreEll.Api.Models;
using TechStoreEll.Api.Services;

namespace TechStoreEll.Api.Controllers;

[AuthorizeRole("Admin")]
public class BackupsController(
    IGenericRepository<Backup> repository,
    ILogger<BackupsController> logger)
    : EntityController<Backup>(repository, logger);
