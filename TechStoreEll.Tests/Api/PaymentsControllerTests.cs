using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TechStoreEll.Api.Controllers;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Interfaces;

namespace TechStoreEll.Tests.Api;

[TestFixture]
public class PaymentsControllerTests
{
    private Mock<IGenericRepository<Payment>> _mockRepo = null!;
    private Mock<ILogger<PaymentsController>> _mockLogger = null!;
    private PaymentsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockRepo = new Mock<IGenericRepository<Payment>>();
        _mockLogger = new Mock<ILogger<PaymentsController>>();
        _controller = new PaymentsController(_mockRepo.Object, _mockLogger.Object);
    }

    [Test]
    public async Task GetAll_ReturnsOk_WithListOfPayments()
    {
        TestContext.WriteLine("Начинаю тест: GetAll_ReturnsOk_WithListOfPayments");

        var payments = new List<Payment>
        {
            new() { Id = 1, Provider = "ТЕСТОВОЕ ЗНАЧЕНИЕ" },
            new() { Id = 2, Provider = "ТЕСТОВОЕ ЗНАЧЕНИЕ 2" }
        };
        TestContext.WriteLine($"Подготовлено payments: {payments.Count}");

        _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(payments);
        TestContext.WriteLine("Мок репозитория настроен");

        var result = await _controller.GetAll();
        TestContext.WriteLine("Вызван метод контроллера GetAll()");

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        TestContext.WriteLine($"Получен результат: {(okResult?.Value != null ? "не null" : "null")}");

        Assert.That(okResult?.Value, Is.EqualTo(payments));
        TestContext.WriteLine("Тест успешно завершён");
    }

    [Test]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        var payment = new Payment { Id = 1, Provider = "ТЕСТОВОЕ ЗНАЧЕНИЕ" };
        _mockRepo.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(payment);

        var result = await _controller.Get(1);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;

        TestContext.WriteLine("Ожидаемое значение: Id={0}, Provider={1}", payment.Id, payment.Provider);

        if (okResult?.Value is Payment actualPayment)
        {
            TestContext.WriteLine("Фактическое значение: Id={0}, Provider={1}", actualPayment.Id, actualPayment.Provider);
        }
        else
        {
            TestContext.WriteLine("Фактическое значение: null или не Payment");
        }

        Assert.That(okResult?.Value, Is.EqualTo(payment));
    }

    [Test]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        _mockRepo.Setup(repo => repo.GetByIdAsync(999)).ReturnsAsync((Payment?)null);
        var result = await _controller.Get(999);
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }
}
