namespace TechStoreEll.Api.Entities.Views;

public partial class VwOrdersSummary
{
    public int? OrderId { get; set; }

    public string? OrderNumber { get; set; }

    public int? UserId { get; set; }

    public string? UserName { get; set; }

    public string? Status { get; set; }

    public decimal? TotalAmount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public long? ItemsCount { get; set; }
}
