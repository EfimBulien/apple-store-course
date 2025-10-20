using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Interfaces;
using TechStoreEll.Core.Services;

namespace TechStoreEll.Api.Controllers;

public class OrderItemsController(
    IGenericRepository<OrderItem> repository,
    ILogger<OrderItemsController> logger) 
    : EntityController<OrderItem>(repository, logger);
