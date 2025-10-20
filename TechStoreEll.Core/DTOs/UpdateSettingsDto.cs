using System.ComponentModel.DataAnnotations;

namespace TechStoreEll.Core.DTOs;

public class UpdateSettingsDto
{
    [Required]
    public string Theme { get; set; } = "light";
    
    [Range(5, 200)]
    public int ItemsPerPage { get; set; } = 20;
    
    [Required]
    public string DateFormat { get; set; } = "YYYY-MM-DD";
    
    [Required]
    public string NumberFormat { get; set; } = "ru_RU";
}