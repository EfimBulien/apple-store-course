using TechStoreEll.Api.Attributes;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Services;
using TechStoreEll.Core.Services.IServices;

namespace TechStoreEll.Api.Controllers;

[AuthorizeRole("Admin")]
public class ProductVariantsController(
    IGenericRepository<ProductVariant> repository,
    ILogger<ProductVariantsController> logger) 
    : EntityController<ProductVariant>(repository, logger);
