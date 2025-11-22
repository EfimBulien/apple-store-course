using System.Text.Json.Serialization;
using NpgsqlTypes;

namespace TechStoreEll.Core.Entities;

public partial class Product
{
    public int Id { get; set; }

    public string? Sku { get; set; } = null!;

    public string? Name { get; set; } = null!;

    public int? CategoryId { get; set; }

    public string? Description { get; set; }

    public bool Active { get; set; }

    public DateTime CreatedAt { get; set; }

    public decimal? AvgRating { get; set; }

    public int? ReviewsCount { get; set; }

    [JsonIgnore]
    public NpgsqlTsVector? SearchVector { get; set; }

    [JsonIgnore]
    public virtual Category? Category { get; set; }

    [JsonIgnore]
    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = [];
    
    [JsonIgnore]
    public virtual ICollection<Review> Reviews { get; set; } = [];
}
