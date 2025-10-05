using TechStoreEll.Api.Entities;

namespace TechStoreEll.Api.Models;

public partial class OrderItem : IEntity
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ProductVariantId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual ProductVariant ProductVariant { get; set; } = null!;
}
