using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Interfaces;
using TechStoreEll.Core.Services;

namespace TechStoreEll.Api.Controllers;

public class OrdersController(
    IGenericRepository<Order> repository, 
    ILogger<OrdersController> logger) :
    EntityController<Order>(repository, logger);
