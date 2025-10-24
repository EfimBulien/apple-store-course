using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TechStoreEll.Api.Controllers;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Interfaces;

namespace TechStoreEll.Tests.Api;

[TestFixture]
public class RolesControllerTests
{
    private Mock<IGenericRepository<Role>> _mockRepo = null!;
    private Mock<ILogger<RolesController>> _mockLogger = null!;
    private RolesController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockRepo = new Mock<IGenericRepository<Role>>();
        _mockLogger = new Mock<ILogger<RolesController>>();
        _controller = new RolesController(_mockRepo.Object, _mockLogger.Object);
    }

    [Test]
    public async Task GetAll_ReturnsOk_WithListOfRoles()
    {
        TestContext.WriteLine("Начинаю тест: GetAll_ReturnsOk_WithListOfRoles");

        var roles = new List<Role>
        {
            new()
            {
                Id = 1,
                Name = "ТЕСТОВОЕ ЗНАЧЕНИЕ"
            },
            new()
            {
                Id = 2,
                Name = "ТЕСТОВОЕ ЗНАЧЕНИЕ 2"
            }
        };
        TestContext.WriteLine($"Подготовлено roles: {roles.Count}");

        _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(roles);
        TestContext.WriteLine("Мок репозитория настроен");

        var result = await _controller.GetAll();
        TestContext.WriteLine("Вызван метод контроллера GetAll()");

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        TestContext.WriteLine($"Получен результат: {(okResult?.Value != null ? "не null" : "null")}");

        Assert.That(okResult?.Value, Is.EqualTo(roles));
        TestContext.WriteLine("Тест успешно завершён");
    }

    [Test]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        var role = new Role
        {
            Id = 1,
            Name = "ТЕСТОВОЕ ЗНАЧЕНИЕ"
        };
        _mockRepo.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(role);

        var result = await _controller.Get(1);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;

        TestContext.WriteLine("Ожидаемое значение: Id={0}, Name={1}", role.Id, role.Name);

        if (okResult?.Value is Role actualRole)
        {
            TestContext.WriteLine("Фактическое значение: Id={0}, Name={1}", actualRole.Id, actualRole.Name);
        }
        else
        {
            TestContext.WriteLine("Фактическое значение: null или не Role");
        }

        Assert.That(okResult?.Value, Is.EqualTo(role));
    }

    [Test]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        _mockRepo.Setup(repo => repo.GetByIdAsync(999)).ReturnsAsync((Role?)null);
        var result = await _controller.Get(999);
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }
}
