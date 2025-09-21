using Microsoft.AspNetCore.Mvc;
using TechStoreEll.Api.DTOs;
using TechStoreEll.Api.Services;

namespace TechStoreEll.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly JwtService _jwtService;

    public AuthController(AuthService authService, JwtService jwtService)
    {
        _authService = authService;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        var result = await _authService.Register(registerDto);
            
        if (!result)
            return BadRequest("User already exists");

        return Ok("User registered successfully");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var user = await _authService.Authenticate(loginDto);
            
        if (user == null)
            return Unauthorized("Invalid credentials");
        Console.WriteLine("Login Success");
        Console.WriteLine(user.Username);
        var token = _jwtService.GenerateToken(user);
        return Ok(new { Token = $"Bearer {token}" });
    }
}