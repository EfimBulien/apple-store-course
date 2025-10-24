using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TechStoreEll.Api.Controllers;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Interfaces;

namespace TechStoreEll.Tests.Api;

[TestFixture]
public class ReviewsControllerTests
{
    private Mock<IGenericRepository<Review>> _mockRepo = null!;
    private Mock<ILogger<ReviewsController>> _mockLogger = null!;
    private ReviewsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockRepo = new Mock<IGenericRepository<Review>>();
        _mockLogger = new Mock<ILogger<ReviewsController>>();
        _controller = new ReviewsController(_mockRepo.Object, _mockLogger.Object);
    }

    [Test]
    public async Task GetAll_ReturnsOk_WithListOfReviews()
    {
        TestContext.WriteLine("Начинаю тест: GetAll_ReturnsOk_WithListOfReviews");

        var reviews = new List<Review>
        {
            new() {
                Id = 1,
                Comment= "ТЕСТОВОЕ ЗНАЧЕНИЕ",
                Rating = 5
            },
            new()
            {
                Id = 2,
                Comment = "ТЕСТОВОЕ ЗНАЧЕНИЕ 2",
                Rating = 5
            }
        };
        TestContext.WriteLine($"Подготовлено reviews: {reviews.Count}");

        _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(reviews);
        TestContext.WriteLine("Мок репозитория настроен");

        var result = await _controller.GetAll();
        TestContext.WriteLine("Вызван метод контроллера GetAll()");

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        TestContext.WriteLine($"Получен результат: {(okResult?.Value != null ? "не null" : "null")}");

        Assert.That(okResult?.Value, Is.EqualTo(reviews));
        TestContext.WriteLine("Тест успешно завершён");
    }

    [Test]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        var review = new Review
        {
            Id = 1, 
            Comment = "ТЕСТОВОЕ ЗНАЧЕНИЕ",
            Rating = 5
        };
        _mockRepo.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(review);

        var result = await _controller.Get(1);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;

        TestContext.WriteLine("Ожидаемое значение: Id={0}, Comment={1}", review.Id, review.Comment);

        if (okResult?.Value is Review actualReview)
        {
            TestContext.WriteLine("Фактическое значение: Id={0}, Comment={1}", actualReview.Id, actualReview.Comment);
        }
        else
        {
            TestContext.WriteLine("Фактическое значение: null или не Review");
        }

        Assert.That(okResult?.Value, Is.EqualTo(review));
    }

    [Test]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        _mockRepo.Setup(repo => repo.GetByIdAsync(999)).ReturnsAsync((Review?)null);
        var result = await _controller.Get(999);
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }
}
