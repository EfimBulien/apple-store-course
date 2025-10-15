using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Services;
using TechStoreEll.Core.Services.IServices;

namespace TechStoreEll.Api.Controllers;

public class OrderItemsController(
    IGenericRepository<OrderItem> repository,
    ILogger<OrderItemsController> logger) 
    : EntityController<OrderItem>(repository, logger);
