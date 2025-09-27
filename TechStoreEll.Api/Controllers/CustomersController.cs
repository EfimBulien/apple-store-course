using TechStoreEll.Api.Infrastructure.Data;
using TechStoreEll.Api.Models;

namespace TechStoreEll.Api.Controllers;

public class CustomersController(AppDbContext context, ILogger<EntityController<Customer>> logger) :
    EntityController<Customer>(context, logger);
