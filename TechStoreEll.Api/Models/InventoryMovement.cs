using System;
using System.Collections.Generic;

namespace TechStoreEll.Api.Models;

public partial class InventoryMovement
{
    public long Id { get; set; }

    public int ProductVariantId { get; set; }

    public string Warehouse { get; set; } = null!;

    public int ChangeQty { get; set; }

    public string Reason { get; set; } = null!;

    public long? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ProductVariant ProductVariant { get; set; } = null!;
}
