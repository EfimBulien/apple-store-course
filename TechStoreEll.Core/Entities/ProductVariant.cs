using System.Text.Json.Serialization;

namespace TechStoreEll.Core.Entities;

public partial class ProductVariant : IEntity
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string VariantCode { get; set; } = null!;

    public decimal Price { get; set; }

    public string? Color { get; set; }

    public int? StorageGb { get; set; }

    public int? Ram { get; set; }

    [JsonIgnore]
    public virtual ICollection<Inventory> Inventories { get; set; } = [];

    [JsonIgnore]
    public virtual ICollection<InventoryMovement> InventoryMovements { get; set; } = [];

    [JsonIgnore]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = [];

    [JsonIgnore]
    public virtual Product Product { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<ProductImage> ProductImages { get; set; } = [];
}
