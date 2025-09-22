using TechStoreEll.Api.Data;
using TechStoreEll.Api.Models;

namespace TechStoreEll.Api.Controllers;

public class OrderItemsController(AppDbContext context) : EntityController<OrderItem>(context)
{
}
