using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TechStoreEll.Api.Controllers;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Interfaces;

namespace TechStoreEll.Tests.Api;

[TestFixture]
public class OrderItemsControllerTests
{
    private Mock<IGenericRepository<OrderItem>> _mockRepo = null!;
    private Mock<ILogger<OrderItemsController>> _mockLogger = null!;
    private OrderItemsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockRepo = new Mock<IGenericRepository<OrderItem>>();
        _mockLogger = new Mock<ILogger<OrderItemsController>>();
        _controller = new OrderItemsController(_mockRepo.Object, _mockLogger.Object);
    }

    [Test]
    public async Task GetAll_ReturnsOk_WithListOfOrderItems()
    {
        TestContext.WriteLine("Начинаю тест: GetAll_ReturnsOk_WithListOfOrderItems");

        var orderitems = new List<OrderItem>
        {
            new() { Id = 1, Quantity = 23},
            new() { Id = 2, Quantity = 23}
        };
        TestContext.WriteLine($"Подготовлено orderitems: {orderitems.Count}");

        _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(orderitems);
        TestContext.WriteLine("Мок репозитория настроен");

        var result = await _controller.GetAll();
        TestContext.WriteLine("Вызван метод контроллера GetAll()");

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        TestContext.WriteLine($"Получен результат: {(okResult?.Value != null ? "не null" : "null")}");

        Assert.That(okResult?.Value, Is.EqualTo(orderitems));
        TestContext.WriteLine("Тест успешно завершён");
    }

    [Test]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        var orderitem = new OrderItem { Id = 1, Quantity = 23};
        _mockRepo.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(orderitem);

        var result = await _controller.Get(1);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;

        TestContext.WriteLine("Ожидаемое значение: Id={0}, Quantity={1}",
            orderitem.Id, orderitem.Quantity);

        if (okResult?.Value is OrderItem actualOrderItem)
        {
            TestContext.WriteLine("Фактическое значение: Id={0}, Quantity={1}",
                actualOrderItem.Id, actualOrderItem.Quantity);
        }
        else
        {
            TestContext.WriteLine("Фактическое значение: null или не OrderItem");
        }

        Assert.That(okResult?.Value, Is.EqualTo(orderitem));
    }

    [Test]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        _mockRepo.Setup(repo => repo.GetByIdAsync(999)).ReturnsAsync((OrderItem?)null);
        var result = await _controller.Get(999);
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }
}
