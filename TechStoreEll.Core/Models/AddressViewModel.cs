using System.ComponentModel.DataAnnotations;

namespace TechStoreEll.Core.Models;

public class AddressFormModel
{
    public int? Id { get; set; }

    [Display(Name = "Название (дом, работа и т.д.)")]
    [RegularExpression(@"^[a-zA-Zа-яА-ЯёЁ0-9\s\.\,\-\!\(\)]{1,50}$", 
        ErrorMessage = "Название может содержать только буквы, цифры и основные знаки препинания")]
    public string Label { get; set; } = null!;

    [Required, Display(Name = "Страна")]
    [RegularExpression(@"^[a-zA-Zа-яА-ЯёЁ\s\-]{2,100}$", 
        ErrorMessage = "Название страны может содержать только буквы, пробелы и дефисы")]
    public string Country { get; set; } = "Россия";

    [Display(Name = "Регион / Область")]
    [RegularExpression(@"^[a-zA-Zа-яА-ЯёЁ\s\-]{2,100}$", 
        ErrorMessage = "Название региона может содержать только буквы, пробелы и дефисы")]
    public string Region { get; set; } = null!;

    [Required, Display(Name = "Город")]
    [RegularExpression(@"^[a-zA-Zа-яА-ЯёЁ\s\-]{2,100}$", 
        ErrorMessage = "Название города может содержать только буквы, пробелы и дефисы")]
    public string City { get; set; } = null!;

    [Required, Display(Name = "Улица")]
    [RegularExpression(@"^[a-zA-Zа-яА-ЯёЁ0-9\s\.\,\-\/]{2,200}$", 
        ErrorMessage = "Название улицы может содержать буквы, цифры, пробелы и основные разделители")]
    public string Street { get; set; } = null!;

    [Required, Display(Name = "Дом")]
    [RegularExpression(@"^[0-9]{1,5}([\/\\\-][0-9]{1,5})?$", 
        ErrorMessage = "Номер дома должен содержать только цифры, может включать дробь через /,-")]
    public string House { get; set; } = null!;

    [Display(Name = "Квартира / Офис")]
    [RegularExpression(@"^[0-9]{1,5}$", 
        ErrorMessage = "Номер квартиры/офиса должен содержать только цифры")]
    public string Apartment { get; set; } = null!;

    [Display(Name = "Индекс")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Индекс должен содержать ровно 6 цифр")]
    public string Postcode { get; set; } = null!;
}
