using System;
using System.Collections.Generic;

namespace TechStoreEll.Api.Models;

public partial class Inventory
{
    public int Id { get; set; }

    public int ProductVariantId { get; set; }

    public string Warehouse { get; set; } = null!;

    public int Quantity { get; set; }

    public int Reserve { get; set; }

    public virtual ProductVariant ProductVariant { get; set; } = null!;
}
