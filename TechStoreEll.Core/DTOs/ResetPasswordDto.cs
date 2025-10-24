using System.ComponentModel.DataAnnotations;

namespace TechStoreEll.Core.DTOs;

public class ForgotPasswordDto
{
    [Required(ErrorMessage = "Email обязателен")]
    [StringLength(256, ErrorMessage = "Email не должен превышать 256 символов")]
    [EmailAddress(ErrorMessage = "Некорректный формат email")]
    public required string Email { get; set; }
}

public class ResetPasswordDto
{
    public required string Token { get; set; }
    
    [Required(ErrorMessage = "Пароль обязателен")]
    [StringLength(255, MinimumLength = 6, ErrorMessage = "Пароль должен быть от 6 до 255 символов")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,}$", 
        ErrorMessage = "Пароль должен содержать минимум одну заглавную букву, одну строчную букву, одну цифру и один специальный символ")]
    public string? Password { get; set; }
    
    [Required(ErrorMessage = "Подтверждение пароля обязательно")]
    [Compare("Password", ErrorMessage = "Пароли не совпадают")]
    public string? ConfirmPassword { get; set; }
}