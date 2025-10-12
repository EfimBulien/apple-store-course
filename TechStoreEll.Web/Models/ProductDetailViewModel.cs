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
    public List<ProductReviewViewModel> Reviews { get; set; } = [];
    public ReviewViewModel ReviewForm { get; set; } = new();
}

public class ProductImageViewModel
{
    public string ImageUrl { get; set; } = "";
    public string? AltText { get; set; }
    public int? SortOrder { get; set; }
}

public class ProductReviewViewModel
{
    public string AuthorName { get; set; } = "Покупатель";
    public DateTime CreatedAt { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}
