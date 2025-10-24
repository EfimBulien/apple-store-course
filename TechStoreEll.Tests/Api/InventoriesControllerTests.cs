using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TechStoreEll.Api.Controllers;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Interfaces;

namespace TechStoreEll.Tests.Api;

[TestFixture]
public class InventoriesControllerTests
{
    private Mock<IGenericRepository<Inventory>> _mockRepo = null!;
    private Mock<ILogger<InventoriesController>> _mockLogger = null!;
    private InventoriesController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockRepo = new Mock<IGenericRepository<Inventory>>();
        _mockLogger = new Mock<ILogger<InventoriesController>>();
        _controller = new InventoriesController(_mockRepo.Object, _mockLogger.Object);
    }

    [Test]
    public async Task GetAll_ReturnsOk_WithListOfInventories()
    {
        TestContext.WriteLine("Начинаю тест: GetAll_ReturnsOk_WithListOfInventories");

        var inventories = new List<Inventory>
        {
            new() { Id = 1, Quantity = 1, Reserve = 1},
            new() { Id = 2, Quantity = 2, Reserve = 2}
        };
        TestContext.WriteLine($"Подготовлено inventories: {inventories.Count}");

        _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(inventories);
        TestContext.WriteLine("Мок репозитория настроен");

        var result = await _controller.GetAll();
        TestContext.WriteLine("Вызван метод контроллера GetAll()");

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        TestContext.WriteLine($"Получен результат: {(okResult?.Value != null ? "не null" : "null")}");

        Assert.That(okResult?.Value, Is.EqualTo(inventories));
        TestContext.WriteLine("Тест успешно завершён");
    }

    [Test]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        var inventory = new Inventory { Id = 1, Quantity = 2};
        _mockRepo.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(inventory);

        var result = await _controller.Get(1);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;

        TestContext.WriteLine("Ожидаемое значение: Id={0}, Quantity={1}", inventory.Id, inventory.Quantity);

        if (okResult?.Value is Inventory actualInventory)
        {
            TestContext.WriteLine("Фактическое значение: Id={0}, Quantity={1}", actualInventory.Id, actualInventory.Quantity);
        }
        else
        {
            TestContext.WriteLine("Фактическое значение: null или не Inventory");
        }

        Assert.That(okResult?.Value, Is.EqualTo(inventory));
    }

    [Test]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        _mockRepo.Setup(repo => repo.GetByIdAsync(999)).ReturnsAsync((Inventory?)null);
        var result = await _controller.Get(999);
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }
}
