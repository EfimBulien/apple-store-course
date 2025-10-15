using TechStoreEll.Api.Attributes;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Services;
using TechStoreEll.Core.Services.IServices;

namespace TechStoreEll.Api.Controllers;

[AuthorizeRole("Admin", "Customer")]
public class CustomersController(
    IGenericRepository<Customer> repository, 
    ILogger<CustomersController> logger) 
    : EntityController<Customer>(repository, logger);
