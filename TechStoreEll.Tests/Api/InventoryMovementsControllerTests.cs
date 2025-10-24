using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TechStoreEll.Api.Controllers;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Interfaces;

namespace TechStoreEll.Tests.Api;

[TestFixture]
public class InventoryMovementsControllerTests
{
    private Mock<IGenericRepository<InventoryMovement>> _mockRepo = null!;
    private Mock<ILogger<InventoryMovementsController>> _mockLogger = null!;
    private InventoryMovementsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockRepo = new Mock<IGenericRepository<InventoryMovement>>();
        _mockLogger = new Mock<ILogger<InventoryMovementsController>>();
        _controller = new InventoryMovementsController(_mockRepo.Object, _mockLogger.Object);
    }

    [Test]
    public async Task GetAll_ReturnsOk_WithListOfInventoryMovements()
    {
        TestContext.WriteLine("Начинаю тест: GetAll_ReturnsOk_WithListOfInventoryMovements");

        var movements = new List<InventoryMovement>
        {
            new() { Id = 1, Reason = "ТЕСТОВОЕ ЗНАЧЕНИЕ" },
            new() { Id = 2, Reason = "ТЕСТОВОЕ ЗНАЧЕНИЕ 2" }
        };
        TestContext.WriteLine($"Подготовлено movements: {movements.Count}");

        _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(movements);
        TestContext.WriteLine("Мок репозитория настроен");

        var result = await _controller.GetAll();
        TestContext.WriteLine("Вызван метод контроллера GetAll()");

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        TestContext.WriteLine($"Получен результат: {(okResult?.Value != null ? "не null" : "null")}");

        Assert.That(okResult?.Value, Is.EqualTo(movements));
        TestContext.WriteLine("Тест успешно завершён");
    }

    [Test]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        var movement = new InventoryMovement { Id = 1, Reason = "ТЕСТОВОЕ ЗНАЧЕНИЕ" };
        _mockRepo.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(movement);

        var result = await _controller.Get(1);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;

        TestContext.WriteLine("Ожидаемое значение: Id={0}, Reason={1}", movement.Id, movement.Reason);

        if (okResult?.Value is InventoryMovement actualInventoryMovement)
        {
            TestContext.WriteLine("Фактическое значение: Id={0}, Reason={1}", actualInventoryMovement.Id, actualInventoryMovement.Reason);
        }
        else
        {
            TestContext.WriteLine("Фактическое значение: null или не InventoryMovement");
        }

        Assert.That(okResult?.Value, Is.EqualTo(movement));
    }

    [Test]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        _mockRepo.Setup(repo => repo.GetByIdAsync(999)).ReturnsAsync((InventoryMovement?)null);
        var result = await _controller.Get(999);
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }
}
