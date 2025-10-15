using System.ComponentModel.DataAnnotations;

namespace TechStoreEll.Core.DTOs;

public class UpdateUserDto
{
    [Required(ErrorMessage = "Email обязателен")]
    [EmailAddress(ErrorMessage = "Некорректный формат email")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Телефон обязателен")]
    public string Phone { get; set; }

    [Required(ErrorMessage = "Имя обязательно")]
    [StringLength(100, ErrorMessage = "Имя не должно превышать 100 символов")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Фамилия обязательна")]
    [StringLength(100, ErrorMessage = "Фамилия не должна превышать 100 символов")]
    public string LastName { get; set; }

    [StringLength(100, ErrorMessage = "Отчество не должно превышать 100 символов")]
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