using System.ComponentModel.DataAnnotations;
using TechStoreEll.Core.Entities;

namespace TechStoreEll.Core.Models;

public class ProductCreateViewModel
{
    [Required(ErrorMessage = "SKU обязателен")]
    [Display(Name = "SKU товара")]
    public string Sku { get; set; }

    [Required(ErrorMessage = "Название обязательно")]
    [Display(Name = "Название товара")]
    public string Name { get; set; }

    [Display(Name = "Категория")]
    public int? CategoryId { get; set; }

    [Display(Name = "Описание")]
    public string Description { get; set; }

    [Display(Name = "Активный товар")]
    public bool Active { get; set; } = true;

    public List<ProductVariantCreateModel> Variants { get; set; } = [];

    public List<Category> Categories { get; set; } = [];
}

public class ProductVariantCreateModel
{
    [Required(ErrorMessage = "Код варианта обязателен")]
    [Display(Name = "Код варианта")]
    public string VariantCode { get; set; }

    [Required(ErrorMessage = "Цена обязательна")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Цена должна быть больше 0")]
    [Display(Name = "Цена")]
    public decimal Price { get; set; }

    [Display(Name = "Цвет")]
    public string Color { get; set; }

    [Display(Name = "Объем памяти (ГБ)")]
    public int? StorageGb { get; set; }

    [Display(Name = "ОЗУ (ГБ)")]
    public int? Ram { get; set; }
    
    [Display(Name = "Изображения")]
    public List<IFormFile> Images { get; set; } = [];
}