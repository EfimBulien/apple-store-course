using System.ComponentModel.DataAnnotations;

namespace TechStoreEll.Core.Models;

public class AddressFormModel
{
    public int? Id { get; set; }

    [Display(Name = "Название (дом, работа и т.д.)")]
    public string? Label { get; set; }

    [Required, Display(Name = "Страна")]
    public string Country { get; set; } = "Россия";

    [Display(Name = "Регион / Область")]
    public string? Region { get; set; }

    [Required, Display(Name = "Город")]
    public string City { get; set; } = null!;

    [Required, Display(Name = "Улица")]
    public string Street { get; set; } = null!;

    [Required, Display(Name = "Дом")]
    public string House { get; set; } = null!;

    [Display(Name = "Квартира / Офис")]
    public string? Apartment { get; set; }

    [Display(Name = "Индекс")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Индекс должен содержать 6 цифр")]
    public string? Postcode { get; set; }
}