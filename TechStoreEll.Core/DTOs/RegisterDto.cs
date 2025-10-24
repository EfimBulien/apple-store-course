using System.ComponentModel.DataAnnotations;

namespace TechStoreEll.Core.DTOs;

public class RegisterDto
{
    [Required(ErrorMessage = "Логин обязателен")]
    [StringLength(256, MinimumLength = 3, ErrorMessage = "Логин должен быть от 3 до 256 символов")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Логин может содержать только латинские буквы, цифры и нижнее подчеркивание")]
    public required string Username { get; set; }

    [Required(ErrorMessage = "Почтовый адрес обязателен")]
    [StringLength(256, ErrorMessage = "Email не должен превышать 256 символов")]
    [EmailAddress(ErrorMessage = "Некорректный формат email")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Телефон обязателен")]
    [StringLength(256, ErrorMessage = "Телефон не должен превышать 256 символов")]
    [RegularExpression(@"^\+?[0-9\s\-\(\)]{10,20}$", ErrorMessage = "Некорректный формат телефона. Допустимы цифры, пробелы, скобки и дефисы")]
    public required string Phone { get; set; }

    [Required(ErrorMessage = "Пароль обязателен")]
    [StringLength(255, MinimumLength = 6, ErrorMessage = "Пароль должен быть от 6 до 255 символов")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,}$", 
        ErrorMessage = "Пароль должен содержать минимум одну заглавную букву, одну строчную букву, одну цифру и один специальный символ")]
    public required string Password { get; set; }

    [Required(ErrorMessage = "Имя пользователя обязательно")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Имя должно быть от 2 до 100 символов")]
    [RegularExpression(@"^[a-zA-Zа-яА-ЯёЁ\s\-]+$", ErrorMessage = "Имя может содержать только буквы, пробелы и дефисы")]
    public required string FirstName { get; set; }

    [Required(ErrorMessage = "Фамилия пользователя обязательна")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Фамилия должна быть от 2 до 100 символов")]
    [RegularExpression(@"^[a-zA-Zа-яА-ЯёЁ\s\-]+$", ErrorMessage = "Фамилия может содержать только буквы, пробелы и дефисы")]
    public required string LastName { get; set; }

    [StringLength(100, ErrorMessage = "Отчество не должно превышать 100 символов")]
    [RegularExpression(@"^[a-zA-Zа-яА-ЯёЁ\s\-]*$", ErrorMessage = "Отчество может содержать только буквы, пробелы и дефисы")]
    public string? MiddleName { get; set; }
}