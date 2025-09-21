using System;
using System.Collections.Generic;

namespace TechStoreEll.Api.Models;

public partial class VwProductStock
{
    public int? VariantId { get; set; }

    public string? ProductName { get; set; }

    public string? VariantCode { get; set; }

    public long? AvailableQty { get; set; }
}
