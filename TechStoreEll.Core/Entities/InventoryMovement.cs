using System.Text.Json.Serialization;

namespace TechStoreEll.Core.Entities;

public partial class InventoryMovement : IEntity
{
    public int Id { get; set; }

    public int ProductVariantId { get; set; }

    public int WarehouseId { get; set; }

    public int ChangeQty { get; set; }

    public string Reason { get; set; } = null!;

    public int? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    [JsonIgnore]
    public virtual User? CreatedByNavigation { get; set; }

    [JsonIgnore]
    public virtual ProductVariant ProductVariant { get; set; } = null!;

    [JsonIgnore]
    public virtual Warehouse Warehouse { get; set; } = null!;
}
