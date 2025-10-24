using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TechStoreEll.Api.Controllers;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Interfaces;

namespace TechStoreEll.Tests.Api;

[TestFixture]
public class WarehousesControllerTests
{
    private Mock<IGenericRepository<Warehouse>> _mockRepo = null!;
    private Mock<ILogger<WarehousesController>> _mockLogger = null!;
    private WarehousesController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockRepo = new Mock<IGenericRepository<Warehouse>>();
        _mockLogger = new Mock<ILogger<WarehousesController>>();
        _controller = new WarehousesController(_mockRepo.Object, _mockLogger.Object);
    }

    [Test]
    public async Task GetAll_ReturnsOk_WithListOfWarehouses()
    {
        TestContext.WriteLine("Начинаю тест: GetAll_ReturnsOk_WithListOfWarehouses");

        var warehouses = new List<Warehouse>
        {
            new() { Id = 1, Name = "ТЕСТОВОЕ ЗНАЧЕНИЕ" },
            new() { Id = 2, Name = "ТЕСТОВОЕ ЗНАЧЕНИЕ 2" }
        };
        TestContext.WriteLine($"Подготовлено warehouses: {warehouses.Count}");

        _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(warehouses);
        TestContext.WriteLine("Мок репозитория настроен");

        var result = await _controller.GetAll();
        TestContext.WriteLine("Вызван метод контроллера GetAll()");

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        TestContext.WriteLine($"Получен результат: {(okResult?.Value != null ? "не null" : "null")}");

        Assert.That(okResult?.Value, Is.EqualTo(warehouses));
        TestContext.WriteLine("Тест успешно завершён");
    }

    [Test]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        var warehouse = new Warehouse { Id = 1, Name = "ТЕСТОВОЕ ЗНАЧЕНИЕ" };
        _mockRepo.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(warehouse);

        var result = await _controller.Get(1);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;

        TestContext.WriteLine("Ожидаемое значение: Id={0}, Name={1}", warehouse.Id, warehouse.Name);

        if (okResult?.Value is Warehouse actualWarehouse)
        {
            TestContext.WriteLine("Фактическое значение: Id={0}, Name={1}", actualWarehouse.Id, actualWarehouse.Name);
        }
        else
        {
            TestContext.WriteLine("Фактическое значение: null или не Warehouse");
        }

        Assert.That(okResult?.Value, Is.EqualTo(warehouse));
    }

    [Test]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        _mockRepo.Setup(repo => repo.GetByIdAsync(999)).ReturnsAsync((Warehouse?)null);
        var result = await _controller.Get(999);
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }
}
