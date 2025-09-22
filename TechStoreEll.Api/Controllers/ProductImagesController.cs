using TechStoreEll.Api.Data;
using TechStoreEll.Api.Models;

namespace TechStoreEll.Api.Controllers;

public class ProductImagesController(AppDbContext context) : EntityController<ProductImage>(context)
{
}
