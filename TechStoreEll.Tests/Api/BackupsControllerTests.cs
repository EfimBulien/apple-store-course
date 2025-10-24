using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TechStoreEll.Api.Controllers;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Interfaces;

namespace TechStoreEll.Tests.Api;

[TestFixture]
public class BackupsControllerTests
{
    private Mock<IGenericRepository<Backup>> _mockRepo = null!;
    private Mock<ILogger<BackupsController>> _mockLogger = null!;
    private BackupsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockRepo = new Mock<IGenericRepository<Backup>>();
        _mockLogger = new Mock<ILogger<BackupsController>>();
        _controller = new BackupsController(_mockRepo.Object, _mockLogger.Object);
    }

    [Test]
    public async Task GetAll_ReturnsOk_WithListOfBackups()
    {
        TestContext.WriteLine("Начинаю тест: GetAll_ReturnsOk_WithListOfBackups");

        var backups = new List<Backup>
        {
            new()
            {
                Id = 1, 
                Filename = "ТЕСТОВОЕ ЗНАЧЕНИЕ",
                Command = "TEST",
                Note = "test"
            },
            new()
            {
                Id = 2, 
                Filename = "ТЕСТОВОЕ ЗНАЧЕНИЕ 2",
                Command = "TEST 2",
                Note = "test 2"
            }
        };
        TestContext.WriteLine($"Подготовлено backups: {backups.Count}");

        _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(backups);
        TestContext.WriteLine("Мок репозитория настроен");

        var result = await _controller.GetAll();
        TestContext.WriteLine("Вызван метод контроллера GetAll()");

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        TestContext.WriteLine($"Получен результат: {(okResult?.Value != null ? "не null" : "null")}");

        Assert.That(okResult?.Value, Is.EqualTo(backups));
        TestContext.WriteLine("Тест успешно завершён");
    }

    [Test]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        var backup = new Backup
        {
            Id = 1, 
            Filename = "ТЕСТОВОЕ ЗНАЧЕНИЕ",
            Command = "TEST",
            Note = "test"
        };
        _mockRepo.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(backup);

        var result = await _controller.Get(1);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;

        TestContext.WriteLine("Ожидаемое значение: Id={0}, Name={1}", backup.Id, backup.Filename);

        if (okResult?.Value is Backup actualBackup)
        {
            TestContext.WriteLine("Фактическое значение: Id={0}, Name={1}", actualBackup.Id, actualBackup.Filename);
        }
        else
        {
            TestContext.WriteLine("Фактическое значение: null или не Backup");
        }

        Assert.That(okResult?.Value, Is.EqualTo(backup));
    }

    [Test]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        _mockRepo.Setup(repo => repo.GetByIdAsync(999)).ReturnsAsync((Backup?)null);
        var result = await _controller.Get(999);
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }
}
