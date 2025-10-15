namespace TechStoreEll.Core.Entities;

public partial class ProductImage : IEntity
{
    public int Id { get; set; }

    public int ProductVariantId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public string? AltText { get; set; }

    public int? SortOrder { get; set; }

    public bool IsPrimary { get; set; }

    public DateTime UploadedAt { get; set; }

    public virtual ProductVariant ProductVariant { get; set; } = null!;
}
