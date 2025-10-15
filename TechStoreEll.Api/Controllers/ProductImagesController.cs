using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Services;
using TechStoreEll.Core.Services.IServices;

namespace TechStoreEll.Api.Controllers;

public class ProductImagesController(
    IGenericRepository<ProductImage> repository,
    ILogger<ProductImagesController> logger) 
    : EntityController<ProductImage>(repository, logger);
