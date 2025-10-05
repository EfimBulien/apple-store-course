using TechStoreEll.Api.Entities;
using TechStoreEll.Api.Services;

namespace TechStoreEll.Api.Controllers;

public class OrderItemsController(
    IGenericRepository<OrderItem> repository,
    ILogger<OrderItemsController> logger) 
    : EntityController<OrderItem>(repository, logger);
