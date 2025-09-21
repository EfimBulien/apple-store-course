using System;
using System.Collections.Generic;

namespace TechStoreEll.Api.Models;

public partial class VwOrdersSummary
{
    public long? OrderId { get; set; }

    public string? OrderNumber { get; set; }

    public long? UserId { get; set; }

    public string? UserName { get; set; }

    public string? Status { get; set; }

    public decimal? TotalAmount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public long? ItemsCount { get; set; }
}
