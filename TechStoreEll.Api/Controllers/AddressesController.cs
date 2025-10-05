using TechStoreEll.Api.Attributes;
using TechStoreEll.Api.Models;
using TechStoreEll.Api.Services;

namespace TechStoreEll.Api.Controllers;

[AuthorizeRole("Admin", "Customer")]
public class AddressesController(
    IGenericRepository<Address> repository,
    ILogger<AddressesController> logger) 
    : EntityController<Address>(repository, logger);
