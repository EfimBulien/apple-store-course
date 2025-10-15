using TechStoreEll.Api.Attributes;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Services;
using TechStoreEll.Core.Services.IServices;

namespace TechStoreEll.Api.Controllers;

[AuthorizeRole("Admin", "Customer")]
public class AddressesController(
    IGenericRepository<Address> repository,
    ILogger<AddressesController> logger) 
    : EntityController<Address>(repository, logger);
