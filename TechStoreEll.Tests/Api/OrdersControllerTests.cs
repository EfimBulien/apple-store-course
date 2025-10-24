using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TechStoreEll.Api.Controllers;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Interfaces;

namespace TechStoreEll.Tests.Api;

[TestFixture]
public class OrdersControllerTests
{
    private Mock<IGenericRepository<Order>> _mockRepo = null!;
    private Mock<ILogger<OrdersController>> _mockLogger = null!;
    private OrdersController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockRepo = new Mock<IGenericRepository<Order>>();
        _mockLogger = new Mock<ILogger<OrdersController>>();
        _controller = new OrdersController(_mockRepo.Object, _mockLogger.Object);
    }

    [Test]
    public async Task GetAll_ReturnsOk_WithListOfOrders()
    {
        TestContext.WriteLine("Начинаю тест: GetAll_ReturnsOk_WithListOfOrders");

        var orders = new List<Order>
        {
            new() { Id = 1, OrderNumber = "ТЕСТОВОЕ ЗНАЧЕНИЕ" },
            new() { Id = 2, OrderNumber = "ТЕСТОВОЕ ЗНАЧЕНИЕ 2" }
        };
        TestContext.WriteLine($"Подготовлено orders: {orders.Count}");

        _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(orders);
        TestContext.WriteLine("Мок репозитория настроен");

        var result = await _controller.GetAll();
        TestContext.WriteLine("Вызван метод контроллера GetAll()");

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        TestContext.WriteLine($"Получен результат: {(okResult?.Value != null ? "не null" : "null")}");

        Assert.That(okResult?.Value, Is.EqualTo(orders));
        TestContext.WriteLine("Тест успешно завершён");
    }

    [Test]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        var order = new Order { Id = 1, OrderNumber = "ТЕСТОВОЕ ЗНАЧЕНИЕ" };
        _mockRepo.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(order);

        var result = await _controller.Get(1);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;

        TestContext.WriteLine("Ожидаемое значение: Id={0}, OrderNumber={1}", order.Id, order.OrderNumber);

        if (okResult?.Value is Order actualOrder)
        {
            TestContext.WriteLine("Фактическое значение: Id={0}, OrderNumber={1}", actualOrder.Id, actualOrder.OrderNumber);
        }
        else
        {
            TestContext.WriteLine("Фактическое значение: null или не Order");
        }

        Assert.That(okResult?.Value, Is.EqualTo(order));
    }

    [Test]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        _mockRepo.Setup(repo => repo.GetByIdAsync(999)).ReturnsAsync((Order?)null);
        var result = await _controller.Get(999);
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }
}
