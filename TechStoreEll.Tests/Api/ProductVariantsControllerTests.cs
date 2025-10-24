using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TechStoreEll.Api.Controllers;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Interfaces;

namespace TechStoreEll.Tests.Api;

[TestFixture]
public class ProductVariantsControllerTests
{
    private Mock<IGenericRepository<ProductVariant>> _mockRepo = null!;
    private Mock<ILogger<ProductVariantsController>> _mockLogger = null!;
    private ProductVariantsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockRepo = new Mock<IGenericRepository<ProductVariant>>();
        _mockLogger = new Mock<ILogger<ProductVariantsController>>();
        _controller = new ProductVariantsController(_mockRepo.Object, _mockLogger.Object);
    }

    [Test]
    public async Task GetAll_ReturnsOk_WithListOfProductVariants()
    {
        TestContext.WriteLine("Начинаю тест: GetAll_ReturnsOk_WithListOfProductVariants");

        var productvariants = new List<ProductVariant>
        {
            new() { Id = 1, VariantCode = "ТЕСТОВОЕ ЗНАЧЕНИЕ" },
            new() { Id = 2, VariantCode = "ТЕСТОВОЕ ЗНАЧЕНИЕ 2" }
        };
        TestContext.WriteLine($"Подготовлено productvariants: {productvariants.Count}");

        _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(productvariants);
        TestContext.WriteLine("Мок репозитория настроен");

        var result = await _controller.GetAll();
        TestContext.WriteLine("Вызван метод контроллера GetAll()");

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        TestContext.WriteLine($"Получен результат: {(okResult?.Value != null ? "не null" : "null")}");

        Assert.That(okResult?.Value, Is.EqualTo(productvariants));
        TestContext.WriteLine("Тест успешно завершён");
    }

    [Test]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        var productvariant = new ProductVariant { Id = 1, VariantCode = "ТЕСТОВОЕ ЗНАЧЕНИЕ" };
        _mockRepo.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(productvariant);

        var result = await _controller.Get(1);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;

        TestContext.WriteLine("Ожидаемое значение: Id={0}, VariantCode={1}", productvariant.Id, productvariant.VariantCode);

        if (okResult?.Value is ProductVariant actualProductVariant)
        {
            TestContext.WriteLine("Фактическое значение: Id={0}, VariantCode={1}", actualProductVariant.Id, actualProductVariant.VariantCode);
        }
        else
        {
            TestContext.WriteLine("Фактическое значение: null или не ProductVariant");
        }

        Assert.That(okResult?.Value, Is.EqualTo(productvariant));
    }

    [Test]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        _mockRepo.Setup(repo => repo.GetByIdAsync(999)).ReturnsAsync((ProductVariant?)null);
        var result = await _controller.Get(999);
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }
}
