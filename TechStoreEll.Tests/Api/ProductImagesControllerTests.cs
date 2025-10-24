using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TechStoreEll.Api.Controllers;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Interfaces;

namespace TechStoreEll.Tests.Api;

[TestFixture]
public class ProductImagesControllerTests
{
    private Mock<IGenericRepository<ProductImage>> _mockRepo = null!;
    private Mock<ILogger<ProductImagesController>> _mockLogger = null!;
    private ProductImagesController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockRepo = new Mock<IGenericRepository<ProductImage>>();
        _mockLogger = new Mock<ILogger<ProductImagesController>>();
        _controller = new ProductImagesController(_mockRepo.Object, _mockLogger.Object);
    }

    [Test]
    public async Task GetAll_ReturnsOk_WithListOfProductImages()
    {
        TestContext.WriteLine("Начинаю тест: GetAll_ReturnsOk_WithListOfProductImages");

        var productimages = new List<ProductImage>
        {
            new() { Id = 1, ImageUrl = "ТЕСТОВОЕ ЗНАЧЕНИЕ" },
            new() { Id = 2, ImageUrl = "ТЕСТОВОЕ ЗНАЧЕНИЕ 2" }
        };
        TestContext.WriteLine($"Подготовлено productimages: {productimages.Count}");

        _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(productimages);
        TestContext.WriteLine("Мок репозитория настроен");

        var result = await _controller.GetAll();
        TestContext.WriteLine("Вызван метод контроллера GetAll()");

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        TestContext.WriteLine($"Получен результат: {(okResult?.Value != null ? "не null" : "null")}");

        Assert.That(okResult?.Value, Is.EqualTo(productimages));
        TestContext.WriteLine("Тест успешно завершён");
    }

    [Test]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        var productimage = new ProductImage { Id = 1, ImageUrl = "ТЕСТОВОЕ ЗНАЧЕНИЕ" };
        _mockRepo.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(productimage);

        var result = await _controller.Get(1);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;

        TestContext.WriteLine("Ожидаемое значение: Id={0}, ImageUrl={1}", productimage.Id, productimage.ImageUrl);

        if (okResult?.Value is ProductImage actualProductImage)
        {
            TestContext.WriteLine("Фактическое значение: Id={0}, ImageUrl={1}", actualProductImage.Id, actualProductImage.ImageUrl);
        }
        else
        {
            TestContext.WriteLine("Фактическое значение: null или не ProductImage");
        }

        Assert.That(okResult?.Value, Is.EqualTo(productimage));
    }

    [Test]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        _mockRepo.Setup(repo => repo.GetByIdAsync(999)).ReturnsAsync((ProductImage?)null);
        var result = await _controller.Get(999);
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }
}
