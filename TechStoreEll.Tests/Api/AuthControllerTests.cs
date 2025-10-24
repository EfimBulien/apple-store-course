using Microsoft.AspNetCore.Mvc;
using Moq;
using TechStoreEll.Api.Controllers;
using TechStoreEll.Core.DTOs;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Services;

namespace TechStoreEll.Tests.Api;

[TestFixture]
public class AuthControllerTests
{
    private Mock<IAuthService> _mockAuthService = null!;
    private Mock<IJwtService> _mockJwtService = null!;
    private AuthController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockJwtService = new Mock<IJwtService>();
        _controller = new AuthController(_mockAuthService.Object, _mockJwtService.Object);
    }

    #region Register Tests

    [Test]
    public async Task Register_ValidDto_ReturnsOk()
    {
        TestContext.WriteLine("Начинаю тест: Register_ValidDto_ReturnsOk");

        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "SecurePass123!",
            Username = "TestUser",
            Phone = "89175221992",
            FirstName = "Тест Пользователь",
            LastName = "Тест Пользователь",
            MiddleName = "Тест Пользователь"
        };

        _mockAuthService
            .Setup(s => s.Register(registerDto))
            .ReturnsAsync(true);

        TestContext.WriteLine("Мок AuthService настроен на успешную регистрацию");

        var result = await _controller.Register(registerDto);
        TestContext.WriteLine("Вызван метод Register");

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult?.Value, Is.EqualTo("Регистрация прошла успешно"));
        TestContext.WriteLine("Тест успешно завершён");
    }

    [Test]
    public async Task Register_UserAlreadyExists_ReturnsBadRequest()
    {
        TestContext.WriteLine("Начинаю тест: Register_UserAlreadyExists_ReturnsBadRequest");

        var registerDto = new RegisterDto
        {
            Email = "existing@example.com",
            Password = "Pass123!",
            FirstName = "Существующий",
            Username = "existing",
            Phone = "Существующий",
            LastName = "Существующий",
            MiddleName = "Существующий"
        };

        _mockAuthService
            .Setup(s => s.Register(registerDto))
            .ReturnsAsync(false);

        TestContext.WriteLine("Мок AuthService настроен на отказ (пользователь существует)");

        var result = await _controller.Register(registerDto);
        TestContext.WriteLine("Вызван метод Register");

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequest = result as BadRequestObjectResult;
        Assert.That(badRequest?.Value, Is.EqualTo("Такой пользователь уже существует"));
        TestContext.WriteLine("Получен ожидаемый BadRequest");
    }

    #endregion

    #region Login Tests

    [Test]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        TestContext.WriteLine("Начинаю тест: Login_ValidCredentials_ReturnsOkWithToken");

        var loginDto = new LoginDto
        {
            Username = "user@example.com",
            Password = "CorrectPass123!"
        };

        var user = new User { Id = 1, Email = loginDto.Username, FirstName = "Авторизованный" };
        var fakeToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.xxxxx";

        _mockAuthService
            .Setup(s => s.Authenticate(loginDto))
            .ReturnsAsync(user);

        _mockJwtService
            .Setup(s => s.GenerateToken(user))
            .Returns(fakeToken);

        TestContext.WriteLine("Моки AuthService и JwtService настроены на успешную аутентификацию");

        var result = await _controller.Login(loginDto);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
    
        var response = okResult?.Value as AuthResponseDto;
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Token, Is.EqualTo($"Bearer {fakeToken}"));
        TestContext.WriteLine($"Получен токен: Bearer {fakeToken}");
        TestContext.WriteLine("Тест успешно завершён");
    }

    [Test]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        TestContext.WriteLine("Начинаю тест: Login_InvalidCredentials_ReturnsUnauthorized");

        var loginDto = new LoginDto
        {
            Username = "wrong@example.com",
            Password = "WrongPass"
        };

        _mockAuthService
            .Setup(s => s.Authenticate(loginDto))
            .ReturnsAsync((User?)null);

        TestContext.WriteLine("Мок AuthService настроен на возврат null (неверные данные)");

        var result = await _controller.Login(loginDto);
        TestContext.WriteLine("Вызван метод Login");

        Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
        var unauthorized = result as UnauthorizedObjectResult;
        Assert.That(unauthorized?.Value, Is.EqualTo("Неверные данные"));
        TestContext.WriteLine("Получен ожидаемый Unauthorized");
    }

    #endregion
}