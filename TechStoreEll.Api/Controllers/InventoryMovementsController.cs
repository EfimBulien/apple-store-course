using TechStoreEll.Api.Data;
using TechStoreEll.Api.Models;

namespace TechStoreEll.Api.Controllers;

public class InventoryMovementsController(AppDbContext context) : EntityController<InventoryMovement>(context)
{
}
