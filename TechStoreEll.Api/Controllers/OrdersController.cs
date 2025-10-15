using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Services;
using TechStoreEll.Core.Services.IServices;

namespace TechStoreEll.Api.Controllers;

public class OrdersController(
    IGenericRepository<Order> repository, 
    ILogger<OrdersController> logger) :
    EntityController<Order>(repository, logger);
