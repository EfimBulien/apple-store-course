using NpgsqlTypes;

namespace TechStoreEll.Api.Entities;

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

    public NpgsqlTsVector? SearchVector { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = [];

    public virtual ICollection<Review> Reviews { get; set; } = [];
}
