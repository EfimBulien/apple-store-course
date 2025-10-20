using TechStoreEll.Api.Attributes;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Interfaces;
using TechStoreEll.Core.Services;

namespace TechStoreEll.Api.Controllers;

[AuthorizeRole("Admin")]
public class InventoriesController(
    IGenericRepository<Inventory> repository, 
    ILogger<InventoriesController> logger) 
    : EntityController<Inventory>(repository, logger);
