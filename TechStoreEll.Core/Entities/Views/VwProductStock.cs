namespace TechStoreEll.Core.Entities.Views;

public partial class VwProductStock
{
    public int? VariantId { get; set; }

    public string? ProductName { get; set; }

    public string? VariantCode { get; set; }

    public string? Warehouse { get; set; }

    public int? AvailableQty { get; set; }
}
