namespace TechStoreEll.Api.Entities;

public partial class Inventory : IEntity
{
    public int Id { get; set; }

    public int ProductVariantId { get; set; }

    public int? WarehouseId { get; set; }

    public int Quantity { get; set; }

    public int Reserve { get; set; }

    public virtual ProductVariant ProductVariant { get; set; } = null!;

    public virtual Warehouse? Warehouse { get; set; }
}
