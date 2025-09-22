using TechStoreEll.Api.Data;
using TechStoreEll.Api.Models;

namespace TechStoreEll.Api.Controllers;

public class InventorysController(AppDbContext context) : EntityController<Inventory>(context)
{
}
