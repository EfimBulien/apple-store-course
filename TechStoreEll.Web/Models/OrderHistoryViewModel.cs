namespace TechStoreEll.Web.Models;

public class OrderHistoryViewModel
{
    public List<OrderSummary> Orders { get; set; } = new();
}

public class OrderSummary
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = null!;
    public string Status { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemSummary> Items { get; set; } = new();
    public string? PaymentStatus { get; set; }
}

public class OrderItemSummary
{
    public string ProductName { get; set; } = null!;
    public string? VariantInfo { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? ImageUrl { get; set; }
}