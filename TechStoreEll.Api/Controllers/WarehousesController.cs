using TechStoreEll.Api.Attributes;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Services;
using TechStoreEll.Core.Services.IServices;

namespace TechStoreEll.Api.Controllers;

[AuthorizeRole("Admin")] // доступ только для администратора
public class WarehousesController(
    IGenericRepository<Warehouse> repository, 
    ILogger<WarehousesController> logger) 
    : EntityController<Warehouse> (repository, logger);
