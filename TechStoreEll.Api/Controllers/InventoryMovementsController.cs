using TechStoreEll.Api.Attributes;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Services;
using TechStoreEll.Core.Services.IServices;

namespace TechStoreEll.Api.Controllers;

[AuthorizeRole("Admin")]
public class InventoryMovementsController(
    IGenericRepository<InventoryMovement> repository, 
    ILogger<InventoryMovementsController> logger) 
    : EntityController<InventoryMovement>(repository, logger);
