using System.ComponentModel.DataAnnotations;

namespace TechStoreEll.Core.DTOs;

using System.ComponentModel.DataAnnotations;

public class UpdateUserDto
{
    [Required(ErrorMessage = "Email обязателен")]
    [StringLength(256, ErrorMessage = "Email не должен превышать 256 символов")]
    [EmailAddress(ErrorMessage = "Некорректный формат email")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Телефон обязателен")]
    [StringLength(256, ErrorMessage = "Телефон не должен превышать 256 символов")]
    [RegularExpression(@"^\+?[0-9\s\-\(\)]{10,20}$", ErrorMessage = "Некорректный формат телефона. Допустимы цифры, пробелы, скобки и дефисы")]
    public required string Phone { get; set; }

    [Required(ErrorMessage = "Имя обязательно")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Имя должно быть от 2 до 100 символов")]
    [RegularExpression(@"^[a-zA-Zа-яА-ЯёЁ\s\-]+$", ErrorMessage = "Имя может содержать только буквы, пробелы и дефисы")]
    public required string FirstName { get; set; }

    [Required(ErrorMessage = "Фамилия обязательна")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Фамилия должна быть от 2 до 100 символов")]
    [RegularExpression(@"^[a-zA-Zа-яА-ЯёЁ\s\-]+$", ErrorMessage = "Фамилия может содержать только буквы, пробелы и дефисы")]
    public required string LastName { get; set; }

    [StringLength(100, ErrorMessage = "Отчество не должно превышать 100 символов")]
    [RegularExpression(@"^[a-zA-Zа-яА-ЯёЁ\s\-]*$", ErrorMessage = "Отчество может содержать только буквы, пробелы и дефисы")]
    public string? MiddleName { get; set; }
}

public class UpdateUserSettingsDto
{
    public string? Theme { get; set; }
    public int? ItemsPerPage { get; set; }
    public string? DateFormat { get; set; }
    public string? NumberFormat { get; set; }
    public string? SavedFilters { get; set; }
    public string? Hotkeys { get; set; }
}