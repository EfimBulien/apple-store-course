namespace TechStoreEll.Web.Models;

public class OrderViewModel
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
    public List<OrderItemSummary> Items { get; set; } = [];
    public List<OrderItemWithReview> Reviews { get; set; } = [];
    public string? PaymentStatus { get; set; }
    public bool CanPay => Status == "new" && PaymentStatus == "pending";
    public bool CanComplete => Status == "paid";
    public bool CanReview => Status == "completed";
}

public class OrderItemSummary
{
    public string ProductName { get; set; } = null!;
    public string? VariantInfo { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? ImageUrl { get; set; }
}

public class OrderItemWithReview
{
    public string ProductName { get; set; } = null!;
    public string? VariantInfo { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? ImageUrl { get; set; }
    public int ProductId { get; set; }
    public bool AlreadyReviewed { get; set; }
}