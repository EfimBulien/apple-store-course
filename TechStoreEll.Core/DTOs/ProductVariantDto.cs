namespace TechStoreEll.Core.DTOs;

public class ProductVariantDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; }
    public string VariantCode { get; set; }
    public decimal Price { get; set; }
    public string? Color { get; set; }
    public int? StorageGb { get; set; }
    public int? Ram { get; set; }
}