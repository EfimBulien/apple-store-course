using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Interfaces;

namespace TechStoreEll.Tests;

public abstract class BaseEntityControllerTests<T> where T : class, IEntity, new()
{
    protected Mock<IGenericRepository<T>> MockRepo = null!;
    protected Mock<ILogger> MockLogger = null!;
    protected ControllerBase Controller = null!;

    // Фабрика для создания контроллера (реализуется в наследнике)
    protected abstract ControllerBase CreateController(IGenericRepository<T> repository, ILogger logger);

    [SetUp]
    public void BaseSetup()
    {
        MockRepo = new Mock<IGenericRepository<T>>();
        MockLogger = new Mock<ILogger>();
        Controller = CreateController(MockRepo.Object, MockLogger.Object);
    }

    [Test]
    public virtual async Task GetAll_ReturnsOk_WithListOfEntities()
    {
        var entities = new List<T> { new(), new() };
        MockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(entities);

        var method = Controller.GetType().GetMethod("GetAll");
        Assert.That(method, Is.Not.Null, "Controller must have GetAll method");

        var result = await (Task<ActionResult<IEnumerable<T>>>)method!.Invoke(Controller, null)!;

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result;
        Assert.That(okResult.Value, Is.EqualTo(entities));
    }

    [Test]
    public virtual async Task GetById_ExistingId_ReturnsOk()
    {
        var entity = new T { Id = 1 };
        MockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entity);

        var method = Controller.GetType().GetMethod("GetById", new[] { typeof(int) });
        Assert.That(method, Is.Not.Null);

        var result = await (Task<ActionResult<T>>)method!.Invoke(Controller, new object?[] { 1 })!;

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result;
        Assert.That(okResult.Value, Is.EqualTo(entity));
    }

    [Test]
    public virtual async Task GetById_NonExistingId_ReturnsNotFoundObjectResult()
    {
        MockRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((T?)null);

        var method = Controller.GetType().GetMethod("GetById", new[] { typeof(int) });
        Assert.That(method, Is.Not.Null);

        var result = await (Task<ActionResult<T>>)method!.Invoke(Controller, new object?[] { 999 })!;

        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
        // Можно дополнительно проверить содержимое:
        var notFound = (NotFoundObjectResult)result.Result;
        Assert.That(notFound.Value, Is.Not.Null);
    }
}