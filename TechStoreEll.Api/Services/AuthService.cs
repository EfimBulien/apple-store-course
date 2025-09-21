using Microsoft.EntityFrameworkCore;
using TechStoreEll.Api.Data;
using TechStoreEll.Api.DTOs;
using TechStoreEll.Api.Models;

namespace TechStoreEll.Api.Services;

public class AuthService(AppDbContext context, PasswordHasherService passwordHasherService)
{
    public async Task<bool> Register(RegisterDto registerDto)
    {
        var existingUser = await context.Users
            .FirstOrDefaultAsync(u => u.Username == registerDto.Username || u.Email == registerDto.Email);

        if (existingUser != null)
            return false;
        
        var hashedPassword = passwordHasherService.HashPassword(registerDto.Password);

        var user = new User
        {
            Username = registerDto.Username,
            Email = registerDto.Email,
            PasswordHash = hashedPassword,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            MiddleName = registerDto.MiddleName,
            IsActive = true,
            RoleId = 3
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<User?> Authenticate(LoginDto loginDto)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Username == loginDto.Username);
        
        if (user == null)
            return null;
        
        return !passwordHasherService.VerifyPassword(loginDto.Password, user.PasswordHash) ? null : user;
    }
}
