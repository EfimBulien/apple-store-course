using TechStoreEll.Api.Attributes;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Services;
using TechStoreEll.Core.Services.IServices;

namespace TechStoreEll.Api.Controllers;

[AuthorizeRole("Admin")]
public class InventoriesController(
    IGenericRepository<Inventory> repository, 
    ILogger<InventoriesController> logger) 
    : EntityController<Inventory>(repository, logger);
