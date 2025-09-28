namespace TechStoreEll.Web.Models;

public class ProductDetailViewModel
{
    public int Id { get; set; }
    public string Sku { get; set; } = "";
    public string Name { get; set; } = "";
    public string CategoryName { get; set; } = "";
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? AvgRating { get; set; }
    public int ReviewsCount { get; set; }

    public string? Color { get; set; }
    public int? StorageGb { get; set; }
    public int? Ram { get; set; }

    public List<ProductImageViewModel> Images { get; set; } = [];
    
}

public class ProductImageViewModel
{
    public string ImageUrl { get; set; } = "";
    public string? AltText { get; set; }
    public int? SortOrder { get; set; }
}