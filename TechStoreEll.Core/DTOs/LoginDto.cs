using System.ComponentModel.DataAnnotations;

namespace TechStoreEll.Core.DTOs;

public class LoginDto
{
    [Required(ErrorMessage = "Логин обязателен")]
    public required string Username { get; set; }
    [Required(ErrorMessage = "Пароль обязателен")]
    public required string Password { get; set; }
}