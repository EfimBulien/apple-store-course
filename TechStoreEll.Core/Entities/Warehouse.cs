using System.Text.Json.Serialization;

namespace TechStoreEll.Core.Entities;

public partial class Warehouse : IEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonIgnore]
    public virtual ICollection<Inventory> Inventories { get; set; } = [];
    
    [JsonIgnore]
    public virtual ICollection<InventoryMovement> InventoryMovements { get; set; } = [];
}
