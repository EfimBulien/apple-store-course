namespace TechStoreEll.Core.DTOs;

public class ProductFullDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string VariantCode { get; set; }
    public decimal Price { get; set; }
    public string? Color { get; set; }
    public int? StorageGb { get; set; }
    public int? Ram { get; set; }
    public string ProductSku { get; set; }
    public string ProductName { get; set; }
    public int? CategoryId { get; set; }
    public string? ProductDescription { get; set; }
    public bool ProductActive { get; set; }
    public DateTime ProductCreatedAt { get; set; }
    public decimal? ProductAvgRating { get; set; }
    public int? ProductReviewsCount { get; set; }
}