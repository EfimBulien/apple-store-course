using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TechStoreEll.Api.Controllers;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Interfaces;

namespace TechStoreEll.Tests.Api;

[TestFixture]
public class UserSettingsControllerTests
{
    private Mock<IGenericRepository<UserSetting>> _mockRepo = null!;
    private Mock<ILogger<UserSettingsController>> _mockLogger = null!;
    private UserSettingsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockRepo = new Mock<IGenericRepository<UserSetting>>();
        _mockLogger = new Mock<ILogger<UserSettingsController>>();
        _controller = new UserSettingsController(_mockRepo.Object, _mockLogger.Object);
    }

    [Test]
    public async Task GetAll_ReturnsOk_WithListOfUserSettings()
    {
        TestContext.WriteLine("Начинаю тест: GetAll_ReturnsOk_WithListOfUserSettings");

        var usersettings = new List<UserSetting>
        {
            new() { Id = 1, Theme = "ТЕСТОВОЕ ЗНАЧЕНИЕ" },
            new() { Id = 2, Theme = "ТЕСТОВОЕ ЗНАЧЕНИЕ 2" }
        };
        TestContext.WriteLine($"Подготовлено usersettings: {usersettings.Count}");

        _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(usersettings);
        TestContext.WriteLine("Мок репозитория настроен");

        var result = await _controller.GetAll();
        TestContext.WriteLine("Вызван метод контроллера GetAll()");

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        TestContext.WriteLine($"Получен результат: {(okResult?.Value != null ? "не null" : "null")}");

        Assert.That(okResult?.Value, Is.EqualTo(usersettings));
        TestContext.WriteLine("Тест успешно завершён");
    }

    [Test]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        var usersetting = new UserSetting { Id = 1, Theme = "ТЕСТОВОЕ ЗНАЧЕНИЕ" };
        _mockRepo.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(usersetting);

        var result = await _controller.Get(1);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;

        TestContext.WriteLine("Ожидаемое значение: Id={0}, Theme={1}", usersetting.Id, usersetting.Theme);

        if (okResult?.Value is UserSetting actualUserSetting)
        {
            TestContext.WriteLine("Фактическое значение: Id={0}, Theme={1}", actualUserSetting.Id, actualUserSetting.Theme);
        }
        else
        {
            TestContext.WriteLine("Фактическое значение: null или не UserSetting");
        }

        Assert.That(okResult?.Value, Is.EqualTo(usersetting));
    }

    [Test]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        _mockRepo.Setup(repo => repo.GetByIdAsync(999)).ReturnsAsync((UserSetting?)null);
        var result = await _controller.Get(999);
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }
}
