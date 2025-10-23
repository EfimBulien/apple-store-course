using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TechStoreEll.Api.Controllers;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Interfaces;

namespace TechStoreEll.Tests;

[TestFixture]
public class CategoriesControllerTests
{
    private Mock<IGenericRepository<Category>> _mockRepo = null!;
    private Mock<ILogger<CategoriesController>> _mockLogger = null!;
    private CategoriesController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockRepo = new Mock<IGenericRepository<Category>>();
        _mockLogger = new Mock<ILogger<CategoriesController>>();
        _controller = new CategoriesController(_mockRepo.Object, _mockLogger.Object);
    }
    
    [Test]
    public async Task GetAll_ReturnsOk_WithListOfCategories()
    {
        TestContext.WriteLine("Начинаю тест: GetAll_ReturnsOk_WithListOfCategories");
    
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "ТЕСТОВОЕ ЗНАЧЕНИЕ" },
            new() { Id = 2, Name = "ТЕСТОВОЕ ЗНАЧЕНИЕ 2" }
        };
        TestContext.WriteLine($"Подготовлено категорий: {categories.Count}");

        _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(categories);
        TestContext.WriteLine("Мок репозитория настроен");

        var result = await _controller.GetAll();
        TestContext.WriteLine("Вызван метод контроллера GetAll()");

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        TestContext.WriteLine($"Получен результат: {(okResult?.Value != null ? "не null" : "null")}");

        Assert.That(okResult?.Value, Is.EqualTo(categories));
        TestContext.WriteLine("Тест успешно завершён");
    }

    [Test]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        var category = new Category { Id = 1, Name = "ТЕСТОВОЕ ЗНАЧЕНИЕ" };
        _mockRepo.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(category);
    
        var result = await _controller.Get(1);
    
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;

        TestContext.WriteLine("Ожидаемое значение: Id={0}, Name={1}", category.Id, category.Name);
    
        if (okResult?.Value is Category actualCategory)
        {
            TestContext.WriteLine("Фактическое значение: Id={0}, Name={1}", actualCategory.Id, actualCategory.Name);
        }
        else
        {
            TestContext.WriteLine("Фактическое значение: null или не Category");
        }

        Assert.That(okResult?.Value, Is.EqualTo(category));
    }

    [Test]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        _mockRepo.Setup(repo => repo.GetByIdAsync(999)).ReturnsAsync((Category?)null);
        var result = await _controller.Get(999);
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }
}