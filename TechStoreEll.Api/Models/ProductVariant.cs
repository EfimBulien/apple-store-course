using TechStoreEll.Api.Entities;

namespace TechStoreEll.Api.Models;

public partial class ProductVariant : IEntity
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string VariantCode { get; set; } = null!;

    public decimal Price { get; set; }

    public string? Color { get; set; }

    public int? StorageGb { get; set; }

    public int? Ram { get; set; }

    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

    public virtual ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
}
