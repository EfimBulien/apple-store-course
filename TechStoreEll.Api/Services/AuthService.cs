using Microsoft.EntityFrameworkCore;
using TechStoreEll.Api.Data;
using TechStoreEll.Api.DTOs;
using TechStoreEll.Api.Models;

namespace TechStoreEll.Api.Services;



    public class AuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Register(RegisterDto registerDto)
        {
            // Проверяем, существует ли пользователь
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == registerDto.Username || u.Email == registerDto.Email);

            if (existingUser != null)
                return false;

            // Создаем нового пользователя (в реальном приложении нужно хэшировать пароль!)
            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = registerDto.Password, //потом перейти на BCrypt
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                MiddleName = registerDto.MiddleName,
                IsActive = true,
                RoleId = 3
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<User?> Authenticate(LoginDto loginDto)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == loginDto.Username && u.PasswordHash == loginDto.Password);
        }
    }
