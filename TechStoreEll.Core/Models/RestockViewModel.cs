using System.Text.Json.Serialization;

namespace TechStoreEll.Core.Models;

public class RestockViewModel
{
    public List<RestockItemModel> Items { get; set; } = [];
    public List<InventoryViewModel> Inventory { get; set; } = new();
    public List<InventoryMovementViewModel> Movements { get; set; } = new();
}

public class InventoryMovementViewModel
{
    public string ProductName { get; set; } = null!;
    public string VariantCode { get; set; } = null!;
    public string WarehouseName { get; set; } = null!;
    public int ChangeQty { get; set; }
    public string Reason { get; set; } = null!;
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class InventoryViewModel
{
    public string ProductName { get; set; } = null!;
    public string VariantCode { get; set; } = null!;
    public string WarehouseName { get; set; } = "—";
    public int Quantity { get; set; }
    public int Reserve { get; set; }
}

public class RestockItemModel
{
    [JsonPropertyName("variant_id")]
    public int ProductVariantId { get; set; }
    
    [JsonPropertyName("warehouse_name")]
    public string WarehouseName { get; set; } = null!;
    
    [JsonPropertyName("qty")]
    public int Quantity { get; set; }
}