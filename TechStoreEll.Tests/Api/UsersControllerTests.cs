using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TechStoreEll.Api.Controllers;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Interfaces;

namespace TechStoreEll.Tests.Api;

[TestFixture]
public class UsersControllerTests
{
    private Mock<IGenericRepository<User>> _mockRepo = null!;
    private Mock<ILogger<UsersController>> _mockLogger = null!;
    private UsersController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockRepo = new Mock<IGenericRepository<User>>();
        _mockLogger = new Mock<ILogger<UsersController>>();
        _controller = new UsersController(_mockRepo.Object, _mockLogger.Object);
    }

    [Test]
    public async Task GetAll_ReturnsOk_WithListOfUsers()
    {
        TestContext.WriteLine("Начинаю тест: GetAll_ReturnsOk_WithListOfUsers");

        var users = new List<User>
        {
            new() { Id = 1, Username = "ТЕСТОВОЕ ЗНАЧЕНИЕ" },
            new() { Id = 2, Username = "ТЕСТОВОЕ ЗНАЧЕНИЕ 2" }
        };
        TestContext.WriteLine($"Подготовлено users: {users.Count}");

        _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(users);
        TestContext.WriteLine("Мок репозитория настроен");

        var result = await _controller.GetAll();
        TestContext.WriteLine("Вызван метод контроллера GetAll()");

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        TestContext.WriteLine($"Получен результат: {(okResult?.Value != null ? "не null" : "null")}");

        Assert.That(okResult?.Value, Is.EqualTo(users));
        TestContext.WriteLine("Тест успешно завершён");
    }

    [Test]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        var user = new User { Id = 1, Username = "ТЕСТОВОЕ ЗНАЧЕНИЕ" };
        _mockRepo.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(user);

        var result = await _controller.Get(1);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;

        TestContext.WriteLine("Ожидаемое значение: Id={0}, Username={1}", user.Id, user.Username);

        if (okResult?.Value is User actualUser)
        {
            TestContext.WriteLine("Фактическое значение: Id={0}, Username={1}", actualUser.Id, actualUser.Username);
        }
        else
        {
            TestContext.WriteLine("Фактическое значение: null или не User");
        }

        Assert.That(okResult?.Value, Is.EqualTo(user));
    }

    [Test]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        _mockRepo.Setup(repo => repo.GetByIdAsync(999)).ReturnsAsync((User?)null);
        var result = await _controller.Get(999);
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }
}
