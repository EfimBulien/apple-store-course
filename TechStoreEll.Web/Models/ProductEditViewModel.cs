using TechStoreEll.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace TechStoreEll.Web.Models;

public class ProductEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Название обязательно")]
    [StringLength(255, ErrorMessage = "Название не должно превышать 255 символов")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Артикул (SKU) обязателен")]
    [StringLength(100, ErrorMessage = "SKU не должен превышать 100 символов")]
    public string Sku { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Описание слишком длинное")]
    public string? Description { get; set; }

    public bool Active { get; set; }

    [Required(ErrorMessage = "Выберите категорию")]
    public int CategoryId { get; set; }

    public List<Category> Categories { get; set; } = new();

    [Url(ErrorMessage = "Некорректный URL изображения")]
    public string? ImageUrl { get; set; }

    public List<ProductVariantEditModel> Variants { get; set; } = new();
}

public class ProductVariantEditModel
{
    public int Id { get; set; }
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Код варианта обязателен")]
    public string VariantCode { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "Цена должна быть положительной")]
    public decimal Price { get; set; }

    public string? Color { get; set; }
    public int? StorageGb { get; set; }
    public int? Ram { get; set; }
    public List<ProductImageEditModel> Images { get; set; } = new();
}

public class ProductImageEditModel
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }
}