using TechStoreEll.Api.Infrastructure.Data;
using TechStoreEll.Api.Models;

namespace TechStoreEll.Api.Controllers;

public class ProductImagesController(AppDbContext context, ILogger<EntityController<ProductImage>> logger) : 
    EntityController<ProductImage>(context, logger);
