using TechStoreEll.Api.Models;
using TechStoreEll.Api.Services;

namespace TechStoreEll.Api.Controllers;

public class OrdersController(
    IGenericRepository<Order> repository, 
    ILogger<OrdersController> logger) :
    EntityController<Order>(repository, logger);
