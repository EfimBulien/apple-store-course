using TechStoreEll.Api.Attributes;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Interfaces;
using TechStoreEll.Core.Services;

namespace TechStoreEll.Api.Controllers;

[AuthorizeRole("Admin")] // доступ только для администратора
public class WarehousesController(
    IGenericRepository<Warehouse> repository, 
    ILogger<WarehousesController> logger) 
    : EntityController<Warehouse> (repository, logger);
