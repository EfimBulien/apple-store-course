using Microsoft.AspNetCore.Mvc;
using TechStoreEll.Api.DTOs;
using TechStoreEll.Api.Services;

namespace TechStoreEll.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AuthService authService, JwtService jwtService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        var result = await authService.Register(registerDto);
            
        if (!result)
            return BadRequest("Такой пользователь уже существует");

        return Ok("Регистрация прошла успешно");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var user = await authService.Authenticate(loginDto);
        
        if (user == null)
            return Unauthorized("Неверные данные");
        
        var token = jwtService.GenerateToken(user);
        return Ok(new { Token = $"Bearer {token}" });
    }
    
    
}