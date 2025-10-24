using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TechStoreEll.Api.Controllers;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Interfaces;

namespace TechStoreEll.Tests;

[TestFixture]
public class AddressesControllerTests
{
    private Mock<IGenericRepository<Address>> _mockRepo = null!;
    private Mock<ILogger<AddressesController>> _mockLogger = null!;
    private AddressesController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockRepo = new Mock<IGenericRepository<Address>>();
        _mockLogger = new Mock<ILogger<AddressesController>>();
        _controller = new AddressesController(_mockRepo.Object, _mockLogger.Object);
    }

    [Test]
    public async Task GetAll_ReturnsOk_WithListOfAddresses()
    {
        TestContext.WriteLine("Начинаю тест: GetAll_ReturnsOk_WithListOfAddresses");

        var addresses = new List<Address>
        {
            new() { Id = 1, Street = "ТЕСТОВАЯ УЛИЦА 1" },
            new() { Id = 2, Street = "ТЕСТОВАЯ УЛИЦА 2" }
        };
        TestContext.WriteLine($"Подготовлено адресов: {addresses.Count}");

        _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(addresses);
        TestContext.WriteLine("Мок репозитория настроен");

        var result = await _controller.GetAll();
        TestContext.WriteLine("Вызван метод контроллера GetAll()");

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        TestContext.WriteLine($"Получен результат: {(okResult?.Value != null ? "не null" : "null")}");

        Assert.That(okResult?.Value, Is.EqualTo(addresses));
        TestContext.WriteLine("Тест успешно завершён");
    }

    [Test]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        var address = new Address { Id = 1, Street = "ТЕСТОВАЯ УЛИЦА" };
        _mockRepo.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(address);

        var result = await _controller.Get(1);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;

        TestContext.WriteLine("Ожидаемое значение: Id={0}, Street={1}", address.Id, address.Street);

        if (okResult?.Value is Address actualAddress)
        {
            TestContext.WriteLine("Фактическое значение: Id={0}, Street={1}", actualAddress.Id, actualAddress.Street);
        }
        else
        {
            TestContext.WriteLine("Фактическое значение: null или не Address");
        }

        Assert.That(okResult?.Value, Is.EqualTo(address));
    }

    [Test]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        TestContext.WriteLine("Начинаю тест: GetById_NonExistingId_ReturnsNotFound");

        _mockRepo.Setup(repo => repo.GetByIdAsync(999)).ReturnsAsync((Address?)null);
        TestContext.WriteLine("Мок репозитория настроен на возврат null для ID=999");

        var result = await _controller.Get(999);
        TestContext.WriteLine("Вызван метод контроллера Get(999)");

        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
        TestContext.WriteLine("Получен ожидаемый результат: NotFound");
    }
}