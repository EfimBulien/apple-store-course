using TechStoreEll.Api.Attributes;
using TechStoreEll.Api.Entities;
using TechStoreEll.Api.Services;

namespace TechStoreEll.Api.Controllers;

[AuthorizeRole("Admin")] // доступ только для администратора
public class WarehousesController(
    IGenericRepository<Warehouse> repository, 
    ILogger<WarehousesController> logger) 
    : EntityController<Warehouse> (repository, logger);
