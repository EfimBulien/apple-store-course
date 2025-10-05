using TechStoreEll.Api.Attributes;
using TechStoreEll.Api.Entities;
using TechStoreEll.Api.Services;

namespace TechStoreEll.Api.Controllers;

[AuthorizeRole("Admin")]
public class InventoriesController(
    IGenericRepository<Inventory> repository, 
    ILogger<InventoriesController> logger) 
    : EntityController<Inventory>(repository, logger);
