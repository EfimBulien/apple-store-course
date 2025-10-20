using TechStoreEll.Api.Attributes;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Interfaces;
using TechStoreEll.Core.Services;

namespace TechStoreEll.Api.Controllers;

[AuthorizeRole("Admin")]
public class InventoryMovementsController(
    IGenericRepository<InventoryMovement> repository, 
    ILogger<InventoryMovementsController> logger) 
    : EntityController<InventoryMovement>(repository, logger);
