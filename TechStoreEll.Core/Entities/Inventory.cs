using System.Text.Json.Serialization;

namespace TechStoreEll.Core.Entities;

public partial class Inventory : IEntity
{
    public int Id { get; set; }

    public int ProductVariantId { get; set; }

    public int? WarehouseId { get; set; }

    public int Quantity { get; set; }

    public int Reserve { get; set; }

    [JsonIgnore]
    public virtual ProductVariant ProductVariant { get; set; } = null!;

    [JsonIgnore]
    public virtual Warehouse? Warehouse { get; set; }
}
