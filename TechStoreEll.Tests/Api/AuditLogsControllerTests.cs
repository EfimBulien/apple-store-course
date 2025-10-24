using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TechStoreEll.Api.Controllers;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Interfaces;

namespace TechStoreEll.Tests.Api;

[TestFixture]
public class AuditLogsControllerTests
{
    private Mock<IGenericRepository<AuditLog>> _mockRepo = null!;
    private Mock<ILogger<AuditLogsController>> _mockLogger = null!;
    private AuditLogsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockRepo = new Mock<IGenericRepository<AuditLog>>();
        _mockLogger = new Mock<ILogger<AuditLogsController>>();
        _controller = new AuditLogsController(_mockRepo.Object, _mockLogger.Object);
    }

    [Test]
    public async Task GetAll_ReturnsOk_WithListOfAuditLogs()
    {
        TestContext.WriteLine("Начинаю тест: GetAll_ReturnsOk_WithListOfAuditLogs");

        var auditlogs = new List<AuditLog>
        {
            new() { 
                Id = 1, 
                TableName = "ТЕСТОВОЕ ЗНАЧЕНИЕ",
                Operation = 'I',
                RecordId = "1", 
                ChangedBy = 1
            },
            new() { 
                Id = 2, 
                TableName = "ТЕСТОВОЕ ЗНАЧЕНИЕ 2",
                Operation = 'I',
                RecordId = "2", 
                ChangedBy = 2
            },
        };
        TestContext.WriteLine($"Подготовлено auditlogs: {auditlogs.Count}");

        _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(auditlogs);
        TestContext.WriteLine("Мок репозитория настроен");

        var result = await _controller.GetAll();
        TestContext.WriteLine("Вызван метод контроллера GetAll()");

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        TestContext.WriteLine($"Получен результат: {(okResult?.Value != null ? "не null" : "null")}");

        Assert.That(okResult?.Value, Is.EqualTo(auditlogs));
        TestContext.WriteLine("Тест успешно завершён");
    }

    [Test]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        var auditlog = new AuditLog
        {
            Id = 1, 
            TableName = "ТЕСТОВОЕ ЗНАЧЕНИЕ",
            Operation = 'I',
            RecordId = "1", 
            ChangedBy = 1
        };
        _mockRepo.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(auditlog);

        var result = await _controller.Get(1);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;

        TestContext.WriteLine("Ожидаемое значение: Id={0}, ={1}", auditlog.Id, auditlog.TableName);

        if (okResult?.Value is AuditLog actualAuditLog)
        {
            TestContext.WriteLine("Фактическое значение: Id={0}, ={1}", actualAuditLog.Id, actualAuditLog.TableName);
        }
        else
        {
            TestContext.WriteLine("Фактическое значение: null или не AuditLog");
        }

        Assert.That(okResult?.Value, Is.EqualTo(auditlog));
    }

    [Test]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        _mockRepo.Setup(repo => repo.GetByIdAsync(999)).ReturnsAsync((AuditLog?)null);
        var result = await _controller.Get(999);
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }
}
