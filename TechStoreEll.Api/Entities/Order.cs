namespace TechStoreEll.Api.Entities;

public partial class Order : IEntity
{
    public int Id { get; set; }

    public string OrderNumber { get; set; } = null!;

    public int UserId { get; set; }

    public string Status { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual User User { get; set; } = null!;
}
