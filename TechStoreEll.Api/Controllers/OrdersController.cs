using TechStoreEll.Api.Infrastructure.Data;
using TechStoreEll.Api.Models;

namespace TechStoreEll.Api.Controllers;

public class OrdersController(AppDbContext context, ILogger<EntityController<Order>> logger) :
    EntityController<Order>(context, logger);
