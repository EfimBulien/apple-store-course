using TechStoreEll.Api.Attributes;
using TechStoreEll.Api.Entities;
using TechStoreEll.Api.Services;

namespace TechStoreEll.Api.Controllers;

[AuthorizeRole("Admin")]
public class ProductVariantsController(
    IGenericRepository<ProductVariant> repository,
    ILogger<ProductVariantsController> logger) 
    : EntityController<ProductVariant>(repository, logger);
