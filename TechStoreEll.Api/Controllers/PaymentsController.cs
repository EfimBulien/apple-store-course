using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Services;
using TechStoreEll.Core.Services.IServices;

namespace TechStoreEll.Api.Controllers;

public class PaymentsController(
    IGenericRepository<Payment> repository, 
    ILogger<PaymentsController> logger)
    : EntityController<Payment>(repository, logger);
