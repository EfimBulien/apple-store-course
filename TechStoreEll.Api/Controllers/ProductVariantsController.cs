using TechStoreEll.Api.Data;
using TechStoreEll.Api.Models;

namespace TechStoreEll.Api.Controllers;

public class ProductVariantsController(AppDbContext context) : EntityController<ProductVariant>(context)
{
}
