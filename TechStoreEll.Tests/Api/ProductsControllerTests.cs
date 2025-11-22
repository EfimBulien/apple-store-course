using Microsoft.AspNetCore.Mvc;
using Moq;
using TechStoreEll.Api.Controllers;
using TechStoreEll.Core.DTOs;
using TechStoreEll.Core.Interfaces;

namespace TechStoreEll.Tests.Api;

[TestFixture]
public class ProductsControllerTests
{
    private Mock<IProductService> _mockService = null!;
    private ProductsController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _mockService = new Mock<IProductService>();
        _controller = new ProductsController(_mockService.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _mockService = null!;
        _controller = null!;
    }

    #region SearchProducts

    [Test]
    public async Task SearchProducts_ReturnsOk_WhenProductsFound()
    {
        var products = new List<ProductFullDto> { new() { Id = 1, ProductName = "Телефон" } };
        _mockService.Setup(s => s.SearchProductVariantsAsync("телефон"))
                    .ReturnsAsync(products);

        var result = await _controller.SearchProducts("Телефон");
        
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        Assert.That(okResult.Value, Is.InstanceOf<List<ProductFullDto>>());
        var returnValue = (List<ProductFullDto>)okResult.Value;
        Assert.That(returnValue.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task SearchProducts_ReturnsNotFound_WhenNoProducts()
    {
        _mockService.Setup(s => s.SearchProductVariantsAsync("несуществующий"))
                    .ReturnsAsync(new List<ProductFullDto>());

        var result = await _controller.SearchProducts("несуществующий");

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    #endregion

    #region GetAllProducts

    [Test]
    public async Task GetAllProducts_ReturnsOk_WhenProductsExist()
    {
        var products = new List<ProductFullDto> { new() { Id = 1 } };
        _mockService.Setup(s => s.GetAllActiveProductVariantsAsync()).ReturnsAsync(products);

        var result = await _controller.GetAllProducts();

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        Assert.That(okResult.Value, Is.InstanceOf<List<ProductFullDto>>());
    }

    [Test]
    public async Task GetAllProducts_ReturnsNotFound_WhenEmpty()
    {
        _mockService.Setup(s => s.GetAllActiveProductVariantsAsync()).ReturnsAsync([]);

        var result = await _controller.GetAllProducts();

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    #endregion

    #region GetProductById

    [Test]
    public async Task GetProductById_ReturnsOk_WhenProductExists()
    {
        var product = new ProductDto { Id = 1, Name = "Ноутбук" };
        _mockService.Setup(s => s.GetProductByIdAsync(1))
                    .ReturnsAsync(product);

        var result = await _controller.GetProductById(1);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        Assert.That(okResult.Value, Is.InstanceOf<ProductDto>());
    }

    [Test]
    public async Task GetProductById_ReturnsNotFound_WhenProductIsNull()
    {
        _mockService.Setup(s => s.GetProductByIdAsync(999))
                    .ReturnsAsync((ProductDto?)null);

        var result = await _controller.GetProductById(999);

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    #endregion

    #region GetProductsByCategory

    [Test]
    public async Task GetProductsByCategory_ReturnsOk_WhenProductsExist()
    {
        var products = new List<ProductDto>
        {
            new() { Id = 1, CategoryId = 5 },
            new() { Id = 2, CategoryId = 5 }
        };
        _mockService.Setup(s => s.SearchProductsAsync(null))
                    .ReturnsAsync(products);

        var result = await _controller.GetProductsByCategory(5);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        var returnValue = (List<ProductDto>)okResult.Value;
        Assert.That(returnValue.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetProductsByCategory_ReturnsNotFound_WhenNoMatchingProducts()
    {
        var products = new List<ProductDto> { new() { Id = 1, CategoryId = 3 } };
        _mockService.Setup(s => s.SearchProductsAsync(null))
                    .ReturnsAsync(products);

        var result = await _controller.GetProductsByCategory(5);

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    #endregion

    #region FilterProducts

    [Test]
    public async Task FilterProducts_ReturnsOk_WithFilteredResult()
    {
        var filtered = new List<ProductFullDto> { new() { Id = 10, Price = 299.99m } };
        _mockService.Setup(s => s.FilterProductsAsync("смартфон", 2, 100, 500, 6, 128))
                    .ReturnsAsync(filtered);

        var result = await _controller.FilterProducts("смартфон", 2, 100, 500, 6, 128);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        var returnValue = (List<ProductFullDto>)okResult.Value;
        Assert.That(returnValue.Count, Is.EqualTo(1));
        Assert.That(returnValue[0].Price, Is.EqualTo(299.99m));
    }

    [Test]
    public async Task FilterProducts_ReturnsOk_EmptyList_WhenNoMatches()
    {
        _mockService.Setup(s => s.FilterProductsAsync("несуществующий", null, null, null, null, null))
                    .ReturnsAsync(new List<ProductFullDto>());

        var result = await _controller.FilterProducts("несуществующий", null, null, null, null, null);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        var returnValue = (List<ProductFullDto>)okResult.Value;
        Assert.That(returnValue.Count, Is.EqualTo(0));
    }

    #endregion

    #region Exception Handling

    [Test]
    public async Task SearchProducts_ReturnsStatusCode500_OnException()
    {
        _mockService.Setup(s => s.SearchProductVariantsAsync("ошибка"))
                    .ThrowsAsync(new InvalidOperationException("DB недоступна"));

        var result = await _controller.SearchProducts("ошибка");

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
    }

    #endregion
}