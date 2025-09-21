namespace TechStoreEll.Api.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Sku { get; set; }
    public string Name { get; set; }
    public int? CategoryId { get; set; }
    public string? Description { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal? AvgRating { get; set; }
    public int? ReviewsCount { get; set; }
    public List<ProductVariantDto> Variants { get; set; } = [];
}
