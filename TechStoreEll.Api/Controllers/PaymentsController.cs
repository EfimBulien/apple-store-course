using TechStoreEll.Api.Attributes;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Interfaces;

namespace TechStoreEll.Api.Controllers;

[AuthorizeRole("Admin")] // доступ только для администратора
public class PaymentsController(
    IGenericRepository<Payment> repository, 
    ILogger<PaymentsController> logger)
    : EntityController<Payment>(repository, logger);
