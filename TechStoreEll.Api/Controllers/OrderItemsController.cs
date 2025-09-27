using TechStoreEll.Api.Infrastructure.Data;
using TechStoreEll.Api.Models;

namespace TechStoreEll.Api.Controllers;

public class OrderItemsController(AppDbContext context, ILogger<EntityController<OrderItem>> logger) : 
    EntityController<OrderItem>(context, logger);
