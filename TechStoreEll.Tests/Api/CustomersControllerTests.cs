using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TechStoreEll.Api.Controllers;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Interfaces;

namespace TechStoreEll.Tests.Api;

[TestFixture]
public class CustomersControllerTests
{
    private Mock<IGenericRepository<Customer>> _mockRepo = null!;
    private Mock<ILogger<CustomersController>> _mockLogger = null!;
    private CustomersController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockRepo = new Mock<IGenericRepository<Customer>>();
        _mockLogger = new Mock<ILogger<CustomersController>>();
        _controller = new CustomersController(_mockRepo.Object, _mockLogger.Object);
    }

    [Test]
    public async Task GetAll_ReturnsOk_WithListOfCustomers()
    {
        TestContext.WriteLine("Начинаю тест: GetAll_ReturnsOk_WithListOfCustomers");

        var customers = new List<Customer>
        {
            new() { Id = 1, ShippingAddressId = 1, BillingAddressId = 1, LoyaltyPoints = 100 },
            new() { Id = 2, ShippingAddressId = 2, BillingAddressId = 2, LoyaltyPoints = 200 }
        };
        TestContext.WriteLine($"Подготовлено customers: {customers.Count}");

        _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(customers);
        TestContext.WriteLine("Мок репозитория настроен");

        var result = await _controller.GetAll();
        TestContext.WriteLine("Вызван метод контроллера GetAll()");

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        TestContext.WriteLine($"Получен результат: {(okResult?.Value != null ? "не null" : "null")}");

        Assert.That(okResult?.Value, Is.EqualTo(customers));
        TestContext.WriteLine("Тест успешно завершён");
    }

    [Test]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        var customer = new Customer { Id = 1, ShippingAddressId = 1, BillingAddressId = 1, LoyaltyPoints = 100 };
        _mockRepo.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(customer);

        var result = await _controller.Get(1);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;

        TestContext.WriteLine("Ожидаемое значение: Id={0}, Name={1}", customer.Id, customer.LoyaltyPoints);

        if (okResult?.Value is Customer actualCustomer)
        {
            TestContext.WriteLine("Фактическое значение: Id={0}, Name={1}", actualCustomer.Id, actualCustomer.LoyaltyPoints);
        }
        else
        {
            TestContext.WriteLine("Фактическое значение: null или не Customer");
        }

        Assert.That(okResult?.Value, Is.EqualTo(customer));
    }

    [Test]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        _mockRepo.Setup(repo => repo.GetByIdAsync(999)).ReturnsAsync((Customer?)null);
        var result = await _controller.Get(999);
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }
}
