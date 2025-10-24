using System.ComponentModel.DataAnnotations;
using TechStoreEll.Core.Entities;

namespace TechStoreEll.Web.Models;

public class ProductCreateViewModel
{
    [Required(ErrorMessage = "SKU обязателен")]
    [StringLength(50, ErrorMessage = "SKU не может быть длиннее 50 символов")]
    [RegularExpression(@"^[A-Z0-9\-_]+$",
        ErrorMessage = "SKU может содержать только заглавные буквы, цифры, дефисы и подчеркивания")]
    [Display(Name = "SKU товара")]
    public string Sku { get; set; } = null!;

    [Required(ErrorMessage = "Название обязательно")]
    [StringLength(200, ErrorMessage = "Название не может быть длиннее 200 символов")]
    [RegularExpression(@"^[a-zA-Zа-яА-Я0-9\s\-_.,!?()]+$",
        ErrorMessage = "Название содержит недопустимые символы")]
    [Display(Name = "Название товара")]
    public string Name { get; set; } = null!;

    [Display(Name = "Категория")]
    public int? CategoryId { get; set; }

    [StringLength(1000, ErrorMessage = "Описание не может быть длиннее 1000 символов")]
    [RegularExpression(@"^[a-zA-Zа-яА-Я0-9\s\-_.,!?()""'&@#%+=:;]*$", 
        ErrorMessage = "Описание содержит недопустимые символы")]
    [Display(Name = "Описание")]
    public string? Description { get; set; }

    [Display(Name = "Активный товар")]
    public bool Active { get; set; } = true;

    [Required(ErrorMessage = "Должен быть хотя бы один вариант товара")]
    [MinLength(1, ErrorMessage = "Должен быть хотя бы один вариант товара")]
    public List<ProductVariantCreateModel> Variants { get; set; } = [];

    public List<Category> Categories { get; set; } = [];
}

public class ProductVariantCreateModel
{
    [Required(ErrorMessage = "Код варианта обязателен")]
    [StringLength(50, ErrorMessage = "Код варианта не может быть длиннее 50 символов")]
    [RegularExpression(@"^[a-zA-Z0-9\-_]+$", 
        ErrorMessage = "Код варианта может содержать только буквы, цифры, дефисы и подчеркивания")]
    [Display(Name = "Код варианта")]
    public string VariantCode { get; set; } = null!;

    [Required(ErrorMessage = "Цена обязательна")]
    [Range(0.01, 1000000, ErrorMessage = "Цена должна быть от 0.01 до 1 000 000")]
    [RegularExpression(@"^\d+(\.\d{1,2})?$", 
        ErrorMessage = "Цена должна быть в формате 123.45")]
    [Display(Name = "Цена")]
    public decimal Price { get; set; }

    [StringLength(30, ErrorMessage = "Цвет не может быть длиннее 30 символов")]
    [RegularExpression(@"^[a-zA-Zа-яА-Я\s\-]+$", 
        ErrorMessage = "Цвет может содержать только буквы, пробелы и дефисы")]
    [Display(Name = "Цвет")]
    public string? Color { get; set; }

    [Range(1, 2000, ErrorMessage = "Объем памяти должен быть от 1 до 2000 ГБ")]
    [Display(Name = "Объем памяти (ГБ)")]
    public int? StorageGb { get; set; }

    [Range(1, 128, ErrorMessage = "ОЗУ должен быть от 1 до 128 ГБ")]
    [Display(Name = "ОЗУ (ГБ)")]
    public int? Ram { get; set; }
    
    [Display(Name = "Изображения")]
    public List<IFormFile> Images { get; set; } = [];
}