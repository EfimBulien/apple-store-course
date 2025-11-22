using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Interfaces;

namespace TechStoreEll.Api.Controllers;

public class ProductImagesController(
    IGenericRepository<ProductImage> repository,
    ILogger<ProductImagesController> logger) 
    : EntityController<ProductImage>(repository, logger);
