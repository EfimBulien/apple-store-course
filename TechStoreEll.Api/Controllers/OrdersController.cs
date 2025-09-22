using TechStoreEll.Api.Data;
using TechStoreEll.Api.Models;

namespace TechStoreEll.Api.Controllers;

public class OrdersController(AppDbContext context) : EntityController<Order>(context)
{
}
