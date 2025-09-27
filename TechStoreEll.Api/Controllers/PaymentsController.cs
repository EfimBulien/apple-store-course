using TechStoreEll.Api.Infrastructure.Data;
using TechStoreEll.Api.Models;

namespace TechStoreEll.Api.Controllers;

public class PaymentsController(AppDbContext context, ILogger<EntityController<Payment>> logger) : 
    EntityController<Payment>(context, logger);
